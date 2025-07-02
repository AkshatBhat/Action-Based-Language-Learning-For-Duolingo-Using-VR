# **Duolingo World**
**Action-Based Language Learning with Immersive VR**

## ❓ **Problem Statement**

Most language-learning platforms rely on memorization and static exercises, leaving learners unprepared for authentic, spontaneous conversations. When faced with real-world situations — ordering at a café, asking for directions — they freeze. Traditional flashcards cannot replicate the speed, accents, and unpredictability of real interactions, making learners feel voiceless and disconnected.

## 💡 **Proposed Solution**

We are developing **Duolingo World** — an **action-based language rehearsal** feature powered by **immersive VR**. Learners will engage in authentic, dynamic scenarios, guided by:

* **Functional Language Deployment** (practicing language chunks in real context)
* **Contextual Reinforcement** (situational learning tied to realistic tasks)
* **Problem-based learning** (navigating out-of-stock items, asking for help)

**Duolingo World** will place learners in realistic **VR scenes** (like grocery stores) where they **interact with NPCs**, **ask questions**, **check out**, and **solve spontaneous challenges** using their target language.

## 🔑 **Key elements include:**

* 🥽 **Immersive VR environments** (Quest 2 hardware)
* 🗣️ **Voice recognition** for conversational practice
* 🎯 **Dynamic quests** with authentic language

This approach bridges the gap between memorized vocabulary and true language fluency — helping learners **live their language**, not just memorize it.

## 🛠️ Tech Stack

### 🎮 Game Engine

* **Unity**

### 🌐 Backend

* **Flask (Python)** – REST API for voice interaction
* **.NET Framework** – Supporting C# scripting and Unity integration

### 🧠 AI & Language Processing

* **Gemini 2.0-Flash API** – For LLM-based conversational responses
* **Google Cloud Speech-to-Text** – For voice transcription (STT)
* **Google Cloud Text-to-Speech** – For audio output (TTS)

### ☁️ Cloud Platform

* **Google Cloud Platform (GCP)** – Hosting APIs and processing services

### 💻 Programming Languages

* **C#** – For Unity scripting and backend interaction
* **Python** – For backend logic and integration

## 🚀 **To Launch**

* Install the newest version of **Unity 6000.1.9f1**
* Pull this repo, click **Add** and **add from disk**, then navigate within `VR_Scene` within the repo and click **Open**
* Click the new reference to the project in the **Hub**
* Press **Play** in the Unity Editor when the project launches with an **Oculus Quest** setup and connected

## 🐞 **Challenges**

* Allowing the **Quest headset to sleep** causes Meta Quest Link to disconnect → Unity and Meta Quest Link need to **restart**
* **VSCode issues** had certain C# scripts referring to **older versions** of other C# scripts leading to **invisible bugs**
* Integrating **Google Cloud Speech-to-Text** and **Text-to-Speech APIs** with Unity
* **Cross-platform issues** with **macOS** and **WindowsOS**

## 🎯 **Future Goals**

* Finish detailing the **VR scene**
* Integrate the **TTS Unity scene** with the VR scene via a **helper NPC**
* **Procedural grocery lists** and **item location** for more thorough learning

## 📈 **Business Impact**

**Duolingo World** can:

* Deepen **user engagement**
* Increase **subscription retention**
* Drive higher **lifetime value**
* Enhance Duolingo’s brand as a leader in **immersive, functional language education**
* Support **responsible tourism** by helping travelers communicate respectfully and engage with local cultures, addressing rising frustration in many cities toward unprepared, non-communicative visitors

## 👥 **Team**

**Kohler UIRP Summer Interns 2025**:

**Aditi Kumar, Akshat Bhat, Caroline Stoklosinski, Deeya Patel, Hongyu Chen, Samuel Hurh**

## 🎥 **YouTube Video Link**

[![Watch the video on YouTube](https://img.youtube.com/vi/9YljTx_-Joc/maxresdefault.jpg)](https://youtu.be/9YljTx_-Joc)

## 📄 **License**

This project is for **demonstration and educational purposes**.

