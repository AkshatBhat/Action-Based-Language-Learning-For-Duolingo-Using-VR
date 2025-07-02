# Duolingo World
Action-Based Language Learning with Immersive VR

## Problem Statement
Most language-learning platforms rely on memorization and static exercises, leaving learners unprepared for authentic, spontaneous conversations. When faced with real-world situations — ordering at a café, asking for directions — they freeze. Traditional flashcards cannot replicate the speed, accents, and unpredictability of real interactions, making learners feel voiceless and disconnected.

## Proposed Solution
We are developing Duolingo World — an action-based language rehearsal feature powered by immersive VR. Learners will engage in authentic, dynamic scenarios, guided by:
- Functional Language Deployment (practicing language chunks in real context)
- Contextual Reinforcement (situational learning tied to realistic tasks)
- Problem-based learning (navigating out-of-stock items, asking for help)

Duolingo World will place learners in realistic VR scenes (like grocery stores) where they interact with NPCs, ask questions, check out, and solve spontaneous challenges using their target language.

## Key elements include:
- Immersive VR environments (Quest 2 hardware)
- Voice recognition for conversational practice
- Dynamic quests with authentic language
- This approach bridges the gap between memorized vocabulary and true language fluency — helping learners live their language, not just memorize it.

## Tech Stack
Unity 6,
Google Cloud Platform,
.NET framework,
C#,
Gemini 2.0-Flash API
Python
Flask API

## To Launch
- Install the newest version of unity 6000.1.9f1
- Pull this repo, click Add and add from disk, then navigate within VR_Scene within the repo and click open
- click the new reference to the project in the hub.
- Press play in the unity editor when the project launches with an oculus quest setup and connected

##Challenges
- Allowing the quest headset to sleep causes meta quest link to disconnect --> unity and meta quest link need to restart
- VSCode issues had certain c# scripts referring to older versions of other c# scripts leading to invisible bugs
- Integrating google cloud speech to text and text to speech APIs with Unity
- Cross-Platform issues with MacOS and WindowsOS

##Future Goals
- Finish detailing the VR scene
- Integrate the tts unity scene with the vr scene via a helper NPC
- Procedural grocery lists and item location for more thorough learning

## Business Impact
Duolingo World can:
- Deepen user engagement
- Increase subscription retention
- Drive higher lifetime value
- Enhance Duolingo’s brand as a leader in immersive, functional language education
- Support responsible tourism by helping travelers communicate respectfully and engage with local cultures, addressing rising frustration in many cities toward unprepared, non-communicative visitors

## Team
AkshatBhat, Cstokl3, samuelHurh, Aditi135, Willc-1, deeyapatel4

Kohler UIRP Summer 2025 


## Youtube Video

## License
This project is for demonstration and educational purposes
