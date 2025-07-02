from fastapi import FastAPI, UploadFile, Form
from fastapi.responses import FileResponse, JSONResponse
from fastapi.staticfiles import StaticFiles
import subprocess
import uuid
import os
import json
import requests

app = FastAPI()

# Ensure 'temp' folder exists
os.makedirs("temp", exist_ok=True)

# Mount temp folder to serve audio files
app.mount("/temp", StaticFiles(directory="temp"), name="temp")

# Set these to your actual paths
GOOGLE_CRED_PATH = "gcloud-key.json"
GEMINI_API_KEY = open("gemini-key.txt").read().strip()

os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = GOOGLE_CRED_PATH

@app.post("/process_audio")
async def process_audio(audio: UploadFile, language: str = Form("es-ES")):
    print("📥 Received audio file...")

    audio_path = os.path.join("temp", f"{uuid.uuid4()}.wav")
    with open(audio_path, "wb") as f:
        f.write(await audio.read())
    print(f"🔊 Saved to {audio_path}")

    transcript = run_stt(audio_path, language)
    print(f"📝 Transcript: {transcript}")

    response_text = run_gemini(transcript)
    print(f"🤖 Gemini says: {response_text}")

    output_path = run_tts(response_text, language)
    print(f"📤 TTS saved to {output_path}")

    return JSONResponse({
        "text": response_text,
        "transcript": transcript,
        "audio": output_path
    })

def run_stt(audio_path, language):
    from google.cloud import speech
    client = speech.SpeechClient()

    with open(audio_path, "rb") as f:
        audio_data = f.read()

    audio = speech.RecognitionAudio(content=audio_data)
    config = speech.RecognitionConfig(
        encoding=speech.RecognitionConfig.AudioEncoding.LINEAR16,
        sample_rate_hertz=16000,
        language_code=language
    )

    response = client.recognize(config=config, audio=audio)
    for result in response.results:
        return result.alternatives[0].transcript
    return ""

def run_gemini(transcript):
    prompt = f"""
Eres un asistente de tienda amigable ayudando a personas que están aprendiendo español.
Tu trabajo es responder solo en español, de forma muy breve, clara y sencilla — como si hablaras con un principiante.
Usa frases cortas de máximo 1-2 líneas.
El cliente dijo: \"{transcript}\". ¿Qué responderías tú?
"""

    headers = {
        "Content-Type": "application/json",
        "x-goog-api-key": GEMINI_API_KEY
    }
    data = {
        "contents": [{
            "role": "user",
            "parts": [{"text": prompt}]
        }]
    }

    r = requests.post(
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
        headers=headers, json=data
    )
    try:
        j = r.json()
        return j["candidates"][0]["content"]["parts"][0]["text"]
    except Exception as e:
        print("❌ Gemini Error:", r.status_code, r.text)
        return "Lo siento, hubo un error procesando tu solicitud."
    
def run_tts(text, language):
    from google.cloud import texttospeech
    client = texttospeech.TextToSpeechClient()

    synthesis_input = texttospeech.SynthesisInput(text=text)
    voice = texttospeech.VoiceSelectionParams(
        language_code=language,
        ssml_gender=texttospeech.SsmlVoiceGender.FEMALE,
    )
    audio_config = texttospeech.AudioConfig(audio_encoding=texttospeech.AudioEncoding.LINEAR16)

    response = client.synthesize_speech(input=synthesis_input, voice=voice, audio_config=audio_config)

    output_path = os.path.join("temp", f"response_{uuid.uuid4()}.wav")
    with open(output_path, "wb") as out:
        out.write(response.audio_content)
    return output_path
