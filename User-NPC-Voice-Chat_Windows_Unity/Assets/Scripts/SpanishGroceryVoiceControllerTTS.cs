using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;

public class SpanishGroceryVoiceControllerTTS : MonoBehaviour
{
    [Header("Spanish Grocery Keywords")]
    public string[] groceryKeywords = {
        // Greetings
        "hola", "buenos días", "buenas tardes",
        
        // Help requests
        "ayuda", "dónde está", "busco", "necesito",
        
        // Common grocery items
        "leche", "pan", "carne", "pollo", "pescado", "huevos",
        "arroz", "pasta", "tomate", "cebolla", "manzana", "plátano",
        "agua", "cerveza", "vino", "café", "azúcar", "sal",
        
        // Sections
        "frutas", "verduras", "lácteos", "carnicería", "panadería", "bebidas",
        
        // Actions
        "comprar", "pagar", "cuánto cuesta", "precio", "oferta",
        "pasillo", "caja", "bolsa", "gracias", "adiós"
    };
    
    [Header("VR Character Response")]
    public AudioSource characterVoice;
    public Animator storeAssistant;
    
    [Header("Text-to-Speech Settings")]
    public bool useTextToSpeech = true;
    public float speechVolume = 0.8f;
    public float speechRate = 1.0f; // Speech speed
    
    // Private variables
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywordActions;
    private int unrecognizedCount = 0;
    private float lastInteractionTime;
    private bool isWaitingForResponse = false;
    
    // Text-to-Speech
    private TextToSpeech textToSpeech;
    
    // Unity Methods
    // void Start()
    // {
    //     SetupTextToSpeech();
    //     SetupGroceryKeywords();
    //     SetupSpanishRecognition();
    //     lastInteractionTime = Time.time;
    // }

    void Start()
    {
        Debug.Log("=== TTS VOICE CONTROLLER DEBUG ===");
        
        SetupTextToSpeech();
        SetupGroceryKeywords();
        SetupSpanishRecognition();
        lastInteractionTime = Time.time;
        
        // ADD THESE DEBUG CHECKS
        Debug.Log($"✅ Keywords loaded: {groceryKeywords.Length}");
        Debug.Log($"✅ KeywordActions count: {keywordActions.Count}");
        Debug.Log($"✅ TextToSpeech component: {(textToSpeech != null ? "Found" : "NULL")}");
        Debug.Log($"✅ CharacterVoice: {(characterVoice != null ? "Found" : "NULL")}");
        
        if (keywordRecognizer != null)
        {
            Debug.Log($"✅ KeywordRecognizer running: {keywordRecognizer.IsRunning}");
            Debug.Log("First few keywords:");
            for (int i = 0; i < Mathf.Min(5, groceryKeywords.Length); i++)
            {
                Debug.Log($"  - '{groceryKeywords[i]}'");
            }
        }
        else
        {
            Debug.LogError("❌ KeywordRecognizer is NULL!");
        }
    }
    
    void SetupTextToSpeech()
    {
        if (useTextToSpeech)
        {
            textToSpeech = gameObject.AddComponent<TextToSpeech>();
            if (characterVoice == null)
            {
                characterVoice = gameObject.AddComponent<AudioSource>();
            }
            characterVoice.volume = speechVolume;
        }
    }
    
    void Update()
    {
        // Proactive help for lost customers
        if (isWaitingForResponse && Time.time - lastInteractionTime > 10f)
        {
            OfferProactiveHelp();
            isWaitingForResponse = false;
        }
        
        // Reset unrecognized counter after successful interaction
        if (Time.time - lastInteractionTime > 30f)
        {
            unrecognizedCount = 0;
        }
    }
    
    void OnDestroy()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
    
    // Setup Methods
    void SetupGroceryKeywords()
    {
        keywordActions = new Dictionary<string, System.Action>
        {
            // Greetings
            {"hola", () => RespondGreeting()},
            {"buenos días", () => RespondGreeting()},
            {"buenas tardes", () => RespondGreeting()},
            
            // Help
            {"ayuda", () => OfferHelp()},
            {"dónde está", () => AskWhatLookingFor()},
            {"busco", () => AskWhatLookingFor()},
            {"necesito", () => AskWhatLookingFor()},
            
            // Products - Dairy
            {"leche", () => DirectToSection("lácteos", "pasillo 3")},
            {"huevos", () => DirectToSection("lácteos", "pasillo 3")},
            
            // Products - Meat
            {"carne", () => DirectToSection("carnicería", "al fondo a la derecha")},
            {"pollo", () => DirectToSection("carnicería", "al fondo a la derecha")},
            {"pescado", () => DirectToSection("pescadería", "junto a la carnicería")},
            
            // Products - Produce
            {"frutas", () => DirectToSection("frutas", "entrada principal")},
            {"verduras", () => DirectToSection("verduras", "entrada principal")},
            {"manzana", () => DirectToSection("frutas", "entrada principal")},
            {"plátano", () => DirectToSection("frutas", "entrada principal")},
            {"tomate", () => DirectToSection("verduras", "entrada principal")},
            {"cebolla", () => DirectToSection("verduras", "entrada principal")},
            
            // Products - Pantry
            {"pan", () => DirectToSection("panadería", "pasillo 1")},
            {"arroz", () => DirectToSection("cereales", "pasillo 2")},
            {"pasta", () => DirectToSection("cereales", "pasillo 2")},
            
            // Products - Beverages
            {"agua", () => DirectToSection("bebidas", "pasillo 4")},
            {"cerveza", () => DirectToSection("bebidas", "pasillo 4")},
            {"vino", () => DirectToSection("bebidas", "pasillo 5")},
            {"café", () => DirectToSection("café", "pasillo 2")},
            
            // Essentials
            {"azúcar", () => DirectToSection("endulzantes", "pasillo 2")},
            {"sal", () => DirectToSection("condimentos", "pasillo 2")},
            
            // Actions
            {"precio", () => HelpWithPrice()},
            {"cuánto cuesta", () => HelpWithPrice()},
            {"pagar", () => DirectToCheckout()},
            {"caja", () => DirectToCheckout()},
            {"oferta", () => ShowSpecials()},
            
            // Goodbye
            {"gracias", () => RespondThanks()},
            {"adiós", () => RespondGoodbye()}
        };
    }
    
    void SetupSpanishRecognition()
    {
        try
        {
            keywordRecognizer = new KeywordRecognizer(groceryKeywords);
            keywordRecognizer.OnPhraseRecognized += OnSpanishKeywordRecognized;
            keywordRecognizer.Start();
            
            Debug.Log("Reconocimiento de voz en español iniciado para tienda");
            SpeakText("Sistema de voz activado. Bienvenido al supermercado virtual.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error configurando reconocimiento: " + e.Message);
        }
    }
    
    // Event Handlers
    private void OnSpanishKeywordRecognized(PhraseRecognizedEventArgs args)
    {
        string keyword = args.text.ToLower();
        ConfidenceLevel confidence = args.confidence;
        
        Debug.Log($"Cliente dijo: '{keyword}' (confianza: {confidence})");
        
        // Reset confusion state on successful recognition
        unrecognizedCount = 0;
        lastInteractionTime = Time.time;
        isWaitingForResponse = false;
        
        if (keywordActions.ContainsKey(keyword))
        {
            keywordActions[keyword].Invoke();
        }
        else
        {
            // Handle partial matches for complex phrases
            HandlePartialMatch(keyword);
        }
    }
    
    private void HandlePartialMatch(string spokenText)
    {
        // Check if spoken text contains any of our keywords
        foreach (var kvp in keywordActions)
        {
            if (spokenText.Contains(kvp.Key))
            {
                Debug.Log($"Coincidencia parcial encontrada: '{kvp.Key}' en '{spokenText}'");
                kvp.Value.Invoke();
                return;
            }
        }
        
        // Default response for unrecognized speech
        RespondDefault();
    }
    
    // Response Methods with TTS
    private void RespondGreeting()
    {
        string response = "¡Hola! Bienvenido a nuestro supermercado. ¿En qué puedo ayudarle?";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Greeting");
    }
    
    private void OfferHelp()
    {
        string response = "¡Por supuesto! ¿Qué producto está buscando?";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Help");
    }
    
    private void AskWhatLookingFor()
    {
        string response = "¿Qué producto necesita encontrar?";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Question");
    }
    
    private void DirectToSection(string product, string location)
    {
        string response = $"{product} está en {location}. Le muestro el camino.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Point");
        ShowDirectionTo(location);
    }
    
    private void HelpWithPrice()
    {
        string response = "Apunte al producto y le diré el precio.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Point");
    }
    
    private void DirectToCheckout()
    {
        string response = "Las cajas están al frente de la tienda. La caja 3 tiene menos fila.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        ShowDirectionTo("cajas");
    }
    
    private void ShowSpecials()
    {
        string response = "Hoy tenemos ofertas en frutas, veinte por ciento descuento en carnes, y dos por uno en lácteos.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
    }
    
    private void RespondThanks()
    {
        string response = "¡De nada! Que tenga un buen día.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Wave");
    }
    
    private void RespondGoodbye()
    {
        string response = "¡Hasta luego! Gracias por visitarnos.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Wave");
    }
    
    private void RespondDefault()
    {
        unrecognizedCount++;
        string response = "";
        
        if (unrecognizedCount == 1)
        {
            response = "Lo siento, no entendí bien. ¿Qué producto está buscando?";
            PlayAnimation("Confused");
            OfferSimpleHelp();
        }
        else if (unrecognizedCount == 2)
        {
            response = "Entiendo que puede ser confuso. ¿Busca comida, bebidas, o productos de limpieza?";
            PlayAnimation("Help");
            OfferCategoryHelp();
        }
        else if (unrecognizedCount >= 3)
        {
            response = "No se preocupe. ¿Quiere que le muestre las secciones de la tienda? Diga sí o señale lo que busca.";
            PlayAnimation("Gesture");
            OfferVisualHelp();
            unrecognizedCount = 0;
        }
        
        Debug.Log("Asistente: " + response);
        SpeakText(response);
    }
    
    // Text-to-Speech Method
    private void SpeakText(string textToSpeak)
    {
        if (useTextToSpeech && textToSpeech != null)
        {
            textToSpeech.StartSpeaking(textToSpeak);
        }
    }
    
    // Confusion Recovery Methods
    private void OfferProactiveHelp()
    {
        string[] helpMessages = {
            "¿Necesita ayuda para encontrar algo específico?",
            "¿Le muestro dónde están las ofertas de hoy?",
            "Si no sabe cómo preguntar, puede señalar productos o decir ayuda",
            "¿Quiere que le enseñe las secciones principales de la tienda?"
        };
        
        int randomIndex = Random.Range(0, helpMessages.Length);
        string response = helpMessages[randomIndex];
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        PlayAnimation("Help");
    }
    
    private void OfferSimpleHelp()
    {
        string response = "Puede decir cosas como busco leche, dónde está el pan, o necesito ayuda.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        isWaitingForResponse = true;
        lastInteractionTime = Time.time;
    }
    
    private void OfferCategoryHelp()
    {
        string response = "Tenemos estas secciones: comida, frutas, carne, lácteos, bebidas, agua, café, limpieza";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        AddTemporaryKeywords(new string[] {"comida", "bebidas", "limpieza"});
        isWaitingForResponse = true;
        lastInteractionTime = Time.time;
    }
    
    private void OfferVisualHelp()
    {
        string response = "Voy a mostrarle un mapa de la tienda. También puede señalar productos con su mano.";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        ShowStoreMap();
        EnablePointAndClick();
    }
    
    private void AddTemporaryKeywords(string[] tempKeywords)
    {
        foreach (string keyword in tempKeywords)
        {
            if (!keywordActions.ContainsKey(keyword))
            {
                switch (keyword)
                {
                    case "comida":
                        keywordActions[keyword] = () => ShowFoodSections();
                        break;
                    case "bebidas":
                        keywordActions[keyword] = () => DirectToSection("bebidas", "pasillo 4 y 5");
                        break;
                    case "limpieza":
                        keywordActions[keyword] = () => DirectToSection("limpieza", "pasillo 6");
                        break;
                }
            }
        }
        RestartRecognizer();
    }
    
    private void ShowFoodSections()
    {
        string response = "Para comida tenemos: frutas y verduras en la entrada, carne al fondo derecha, lácteos pasillo 3, pan pasillo 1. ¿Cuál de estas secciones le interesa?";
        Debug.Log("Asistente: " + response);
        SpeakText(response);
        AddTemporaryKeywords(new string[] {"frutas", "verduras", "carne", "lácteos", "pan"});
    }
    
    // Helper Methods
    private void PlayAnimation(string animationName)
    {
        if (storeAssistant != null)
        {
            storeAssistant.SetTrigger(animationName);
        }
    }
    
    private void ShowDirectionTo(string location)
    {
        Debug.Log($"Mostrando dirección hacia: {location}");
        unrecognizedCount = 0;
    }
    
    private void ShowStoreMap()
    {
        Debug.Log("Mostrando mapa visual de la tienda...");
    }
    
    private void EnablePointAndClick()
    {
        Debug.Log("Modo señalar activado - apunte a productos para información");
    }
    
    private void RestartRecognizer()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
        
        List<string> allKeywords = new List<string>(groceryKeywords);
        foreach (var action in keywordActions)
        {
            if (!allKeywords.Contains(action.Key))
            {
                allKeywords.Add(action.Key);
            }
        }
        
        keywordRecognizer = new KeywordRecognizer(allKeywords.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnSpanishKeywordRecognized;
        keywordRecognizer.Start();
    }
}