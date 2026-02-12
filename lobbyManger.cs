using Fusion;
using Fusion.Sockets;
using HutongGames.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.ProBuilder.MeshOperations;
public class LobbyManager : MonoBehaviour
{
    [Header("Player Card Setup")]
    [SerializeField] private GameObject playerCardPrefab;
    [SerializeField] private NetworkObject botPrefab;

    [Header("Team Panels")]
    [SerializeField] private List<TeamPanel> teamPanels = new();
    public List<LobbyPlayerCardUI> staticCards;
    public static LobbyManager instance;
    public List<LobbyPlayer> humanPlayer = new List<LobbyPlayer>();
    public List<LobbyPlayer> aiPlayer = new List<LobbyPlayer>();
    public List<PlayerInfo> lobbyPlayers = new();
    public int maxPlayer = 8;
    public float testTimer = 30f;
    public bool IsBotInjectionPhase => botInjectionTriggered;
    private bool botInjectionTriggered = false;

    [Header("Development Bypass")]
    public bool bypassLobby = false;

    [SerializeField] private float botInjectionDelay = 10f;
    [SerializeField] private TextMeshProUGUI lobbyTimerText;
    [SerializeField] private Transform[] teamRedSpawnPoints;
    [SerializeField] private Transform[] teamBlueSpawnPoints;

    private float lobbyTimer = 30f;
    // Total of Players
    private readonly Dictionary<LobbyPlayerCardUI, PlayerInfo> _cardOccupant = new();
    private readonly Dictionary<PlayerRef, LobbyPlayerCardUI> _playerCard = new();
    private int _manualPlayersCount;
    private Coroutine countdownRoutine;
    public int totalPlayers => _manualPlayersCount;
    

    [System.Serializable]
    public class TeamPanel
    {
        public int teamId;
        public RectTransform cardParent;
    }

    private Dictionary<int, RectTransform> teamPanelLookup;

    private void Awake()
    {
        
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[LobbyManager] Duplicate instance detected. Destroying new instance.");
            Destroy(gameObject);
            return;
        }

        instance = this; // set it early
        DontDestroyOnLoad(gameObject);
        
        staticCards = GetComponentsInChildren<LobbyPlayerCardUI>(true).ToList();
        //Debug.Log($"[LobbyManager] Found {staticCards.Count} static cards in prefab hierarchy.");
        BuildTeamPanelLookup();
        ValidatePrefabAssignment();
        //lobbyTime();
    }

    private void Start()
    {
        Debug.Log("[LobbyManager] Start() called");

        InitializeStaticCards();
       
           if (countdownRoutine != null)
        { 
            
            StopCoroutine(countdownRoutine);
            Debug.Log("[LobbyManager] Restarting countdown coroutine.");
        }
        Debug.Log("[LobbyManager] Starting countdown coroutine...");
             
        countdownRoutine = StartCoroutine(LobbyCountdownRoutine());
    }

    private void InitializeStaticCards()
    {
        Debug.Log("[LobbyManager] Initializing static cards with placeholder data...");

        foreach (var card in staticCards)
        {
            int teamId = GetTeamIdForCard(card);
            int slotIndex = GetSlotIndexWithinTeam(card, teamId);

            var placeholderInfo = new PlayerInfo(
                name: "",
                id: slotIndex,         // use per-team slot index as initial card id
                teamID: teamId,
                isBot: false,
                isReady: false
            );

            card.Setup(placeholderInfo);
            // placeholders are not considered occupied
            if (_cardOccupant.ContainsKey(card)) _cardOccupant.Remove(card); continue;
        }
    }
    private void BuildTeamPanelLookup()
    {
        teamPanelLookup = new Dictionary<int, RectTransform>();

        foreach (var panel in teamPanels)
        {
            if (panel.cardParent == null)
            {
                Debug.LogWarning($"[LobbyManager] Missing card parent for team {panel.teamId}");
                continue;
            }

            if (teamPanelLookup.ContainsKey(panel.teamId))
            {
                Debug.LogWarning($"[LobbyManager] Duplicate team ID: {panel.teamId}");
                continue;
            }

            teamPanelLookup.Add(panel.teamId, panel.cardParent);
        }
    }


    public void UpdateCardForPlayer(PlayerRef playerPref, string name, bool isReady)
    {
        if(_playerCard.TryGetValue(playerPref, out var card) && card != null)
        { 
            card.playerNameIdText.text = string.IsNullOrEmpty(name) ? "Waiting for Player..." : name;
            card.setReadyState(isReady,false);
        }
        else
        {
            Debug.LogWarning($"[LobbyManager] No UI card mapped for PlayerRef {playerPref}");
        }
    }

    public void AddPlayerCard(PlayerInfo info)
    {

        Debug.Log($"[LobbyManager] AddPlayerCard called for {info.PlayerName}, Team={info.teamID}, Ref={info.playerRef}");

        Debug.Log($"[LobbyManager] AddPlayerCard called for {info.PlayerName}, Team={info.teamID}, Ref={info.playerRef}");

        if (info.playerRef != default && hasPlayer(info.playerRef))
        {
            Debug.Log($"[LobbyManager] Player {info.playerRef} already exists. Updating card.");

            // FIX: find the card index and pass it explicitly
            var existingCard = staticCards.FirstOrDefault(c => c.PlayerID == info.playerId);
            if (existingCard != null)
            {
                int slotIndex = staticCards.IndexOf(existingCard);
                AssignToCard(info, slotIndex);
            }
            return;
        }

        var chosenCard = ChooseFreeCardForTeam(info.teamID);
        if (chosenCard == null)
        {
            Debug.LogWarning($"[LobbyManager] No available UI card for team {info.teamID}. Skipping UI injection.");
            return;
        }

        Debug.Log($"[LobbyManager] Chosen card: {chosenCard.name} for {info.PlayerName}");

        int chosenIndex = staticCards.IndexOf(chosenCard);

        info.playerId = chosenCard.PlayerID;
        _cardOccupant[chosenCard] = info;
        if (info.playerRef != default)
            _playerCard[info.playerRef] = chosenCard;

        lobbyPlayers.Add(info);
        AssignToCard(info, chosenIndex);

        // restart timer when a  human logs in

        if(!info.isBot)
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
                Debug.Log("[LobbyManager] Restarting countdown coroutine due to new human player.");
            }
            countdownRoutine = StartCoroutine(LobbyCountdownRoutine());
        }

        Debug.Log($"[LobbyManager] Added PlayerInfo for {info.PlayerName} (Team: {info.teamID}) -> Card '{chosenCard.name}' (ID: {info.playerId})");
    }


    public bool hasPlayer(PlayerRef playerRef)
    {
        Debug.Log($"[LobbyManager] hasPlayer({playerRef})");
        return lobbyPlayers.Any(p => p.playerRef == playerRef 
        && !p.isBot && !string.IsNullOrEmpty(p.PlayerName));
    }

    public LobbyPlayerCardUI GetAvailableCard()
    {
        return staticCards.FirstOrDefault(c => !_cardOccupant.ContainsKey(c));
    }

    public void AssignToCard(PlayerInfo info, int slotIndex)
    {

        Debug.Log($"[LobbyManager] AssignToCard called for {info.PlayerName}, slotIndex={slotIndex}");

        if (slotIndex < 0 || slotIndex >= staticCards.Count)
        {
            Debug.LogError($"Invalid slot index: {slotIndex} for player {info.PlayerName}");
            return;
        }

        Debug.Log($"[LobbyManager] Assigning PlayerInfo to card with PlayerID: {info.playerId}");

        var card = staticCards[slotIndex];
        if (card != null)
        {
            Debug.Log($"[LobbyManager] Setting up card {card.name} for {info.PlayerName}");
            card.Setup(info);
        }
        else
        {
            Debug.LogWarning($"[LobbyManager] No static card found at slot {slotIndex}");
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        //Delay validation slightly to allow Unity to populate serialized fields
        UnityEditor.EditorApplication.delayCall += ValidatePrefabAssignment;
        Debug.Log($"[LobbyManager] OnValidate triggered. Prefab assigned: {playerCardPrefab != null}");
        if (playerCardPrefab == null)
            Debug.LogWarning("[LobbyManager] Player card prefab is not assigned.");

        var seen = new HashSet<int>();
        foreach (var panel in teamPanels)
        {
            if (panel.cardParent == null)
                Debug.LogWarning($"[LobbyManager] Missing card parent for team {panel.teamId}");

            if (!seen.Add(panel.teamId))
                Debug.LogWarning($"[LobbyManager] Duplicate team ID: {panel.teamId}");
        }
    }
#endif
    public void ValidateLobbyCards()
    {
        var allCards = GetComponentsInChildren<LobbyPlayerCardUI>(true);
        foreach (var card in allCards)
        {

            if (card.playerNameIdText == null)
            {
                Debug.LogError($"[Validation] Missing playerNameIdText on {card.name} (InstanceID: {card.GetInstanceID()})");
            }
            else
            {
                Debug.Log($"[Validation] {card.name} is wired correctly.");
            }
        }
    }


    void ValidatePrefabAssignment()
    {
        if (playerCardPrefab == null)
        {
            Debug.LogWarning("[LobbyManager] Player card prefab is not assigned.");
        }
        else
        {
            Debug.Log($"[LobbyManager] Prefab assigned correctly: {playerCardPrefab.name}");
        }
    }

    public void addHumanPlayer(LobbyPlayer player)
    {

        humanPlayer.Add(player);
        _manualPlayersCount++;
        
        var info = new PlayerInfo(
            player.PlayerName, 
            player.Object.InputAuthority.RawEncoded, 
            teamID: _manualPlayersCount % 2, 
            isBot: false,
            isReady: player.IsReady
            )
        {
            playerRef = player.Object.InputAuthority
         };
        if (totalPlayers >= maxPlayer)
        {
            Debug.LogWarning("[LobbyManager] Max player count reached. Skipping injection.");
            return;
        }

        AddPlayerCard(info);
        CheckStartCondition();
    }

    public void addAIPlayer(LobbyPlayer player)
    {
        aiPlayer.Add(player);
         _manualPlayersCount++;

        var info = new PlayerInfo(
            player.PlayerName,
            player.Object.InputAuthority.RawEncoded,
            teamID: _manualPlayersCount % 2,
            isBot: true,
            isReady: player.IsReady
            )

        {
            playerRef = player.Object.InputAuthority
        };

        if (totalPlayers >= maxPlayer)
        {
            Debug.LogWarning("[LobbyManager] Max player count reached. Skipping injection.");
            return;
        }


        AddPlayerCard(info);
        CheckStartCondition();
    }


    public void CheckStartCondition()
    {
        Debug.Log($"[LobbyManger]");
        if (totalPlayers >= maxPlayer && allPlayerReady())
        {
            Debug.Log($"[LobbyManger] Conditions met. Start game...");
            startGame();
        }

        else
        {
            Debug.Log($"[LobbyManger] Conditions not met. Waiting....");
            Debug.Log("NetworkBootstrapper not found");
        }
    }

    public bool allPlayerReady()
    {
        bool humansReady = humanPlayer.All(p => p.IsReady);
        bool simsReady = aiPlayer.All(p => p.IsReady);
        
        Debug.Log($"[LobbyManager] allPlayerReady -> Humans ready: {humansReady}, Sims ready: {simsReady}");
        return humanPlayer.All(p => p.IsReady);
    }
    //change loadlevel here

    public void startGame()
    {
        Debug.Log("[LobbyManager] Attempting to start game...");

        if (NetworkBootstrapper.Runner != null && NetworkBootstrapper.Runner.IsRunning)
        {
            if (UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath("GreyBox") < 0)
            {
                Debug.LogError("[LobbyManager] GreyBox scene is not in Build Settings!");
                return;
            }

            var s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Debug.Log($"[LobbyManager] Runner is live. Current scene index={s.buildIndex}, name={s.name}. Loading GreyBox...");

            var greyBoxScene = SceneRef.FromIndex(3);
            FusionCallbackHandler.SetSceneIndex(greyBoxScene.AsIndex);
            NetworkBootstrapper.Runner.LoadScene(greyBoxScene);
        }
        else
        {
            Debug.LogError("[LobbyManager] Tried to load GreyBox but Runner not running!");
        }
    }



    private void InjectBotPlayer(int index)
    {

        var botInfo = new PlayerInfo(
            BotGeneratorName.generateName(),
            id: -1, // will be replaced by chosen card
            teamID: index % 2, // or however you want to assign teams
            isBot: true,
            isReady: true
        );

        AddPlayerCard(botInfo); // UI only
        Debug.Log($"[LobbyManager] Injected bot UI only: {botInfo.PlayerName}, Team={botInfo.teamID}");
    }

    public Vector3 GetSpawnPositionForTeam(int teamId)
    {
        if (teamId == 0 && teamRedSpawnPoints != null && teamRedSpawnPoints.Length > 0)
        {
            int i = UnityEngine.Random.Range(0, teamRedSpawnPoints.Length);
            return teamRedSpawnPoints[i].position;
        }
        if (teamId == 1 && teamBlueSpawnPoints != null && teamBlueSpawnPoints.Length > 0)
        {
            int i = UnityEngine.Random.Range(0, teamBlueSpawnPoints.Length);
            return teamBlueSpawnPoints[i].position;
        }
        // Fallback if nothing assigned
        return Vector3.zero;
    }

    // New helper returns the actual transform, not just position
    public Transform GetSpawnTransformForTeam(int teamId)
    {
        if (teamId == 0 && teamRedSpawnPoints != null && teamRedSpawnPoints.Length > 0)
        {
            int i = UnityEngine.Random.Range(0, teamRedSpawnPoints.Length);
            return teamRedSpawnPoints[i];
        }
        if (teamId == 1 && teamBlueSpawnPoints != null && teamBlueSpawnPoints.Length > 0)
        {
            int i = UnityEngine.Random.Range(0, teamBlueSpawnPoints.Length);
            return teamBlueSpawnPoints[i];
        }
        // Fallback if nothing assigned
        return null;
    }

    //check if the local player is ready
    private bool LocalPlayerIsReady() 
    { 
       var localPlayer = humanPlayer.FirstOrDefault(p => p.Object.HasInputAuthority); 
       return localPlayer != null && localPlayer.IsReady;
    }

    private Vector3 GetBotSpawnPosition(int index)
    {
        // Replace with your actual spawn logic
       return new Vector3(index * 2f, 0f, 0f); //Spread bots horizontally
    }

    //fix player list
    private bool allHumanPlayerReady()
    { 
        foreach(var player in humanPlayer)
        {
            //if(!player.isBot) return false;
            if (!player.IsReady) return false;
        }
        return true;
    }

    public int AssignTeamForPlayer()
    {
        // count humans on each team
        int redCount = lobbyPlayers.Count(p => p.teamID == 0 && !p.isBot);
        int blueCount = lobbyPlayers.Count(p => p.teamID == 1 && !p.isBot);

        //Prefer the team with fewer players
        if (redCount < blueCount)
        {
            return 0; // Red team
        }
        if (blueCount < redCount)
        {
            return 1; // Blue team

        }

        // if equal, check ui card capacity
        int redSlots = staticCards.Count(c => GetTeamIdForCard(c) == 0);
        int blueSlots = staticCards.Count(c => GetTeamIdForCard(c) == 1);
       
        if (redCount < redSlots)
        {
            return 0; // Red team
        }
        if (blueCount < blueSlots)
        {
            return 1; // Blue team
        }
        // if both full (should not happen), default to 0

        return 0;

    }

    public void lobbyTime()
    {
        StartCoroutine(LobbyCountdownRoutine());
    }

    private float GetCountDownTimer()
    {
        //Development bypass

        if (bypassLobby)
        {
           Debug.Log("[LobbyManager] BypassLobby is enabled. Returning 1f for countdown.");
            return 1f;
        }

        // count human who logged in (not bots)
        int humanCount = lobbyPlayers.Count (p => !p.isBot);

        // count humnan who are ready
        int readyHuman = lobbyPlayers.Count(p => !p.isBot && p.IsReady);


        if (humanCount == 0)
            {
            Debug.Log("[LobbyManager] No human players, injecting bots to start match.");
            return testTimer; //short time for bot only game
            }
        if (readyHuman >= 1 && readyHuman <= 7)
            {
            Debug.Log("[LobbyManager] All human players ready, starting soon.");
            return 30f;
            }
        if (readyHuman >= 8)
            {
            Debug.Log("[LobbyManager] 8+ humans and all ready. Returning 10f.");
            return 10f;
            }

        Debug.Log("[LobbyManager] Default path. returning test timer.");
        return testTimer;//default time 
    }


    private IEnumerator LobbyCountdownRoutine()
    {

       
        Debug.Log("[LobbyManager] LobbyCountdownRoutine started.");

        float timeRemaining = GetCountDownTimer();
        Debug.Log($"[LobbyManager] Initial countdown time = {timeRemaining}");

        /*if (timeRemaining <= 0)
        {
            Debug.Log("[LogManger] Countdown skipped-conditions not met.");
            yield break;
        }*/


        while (timeRemaining > 0f)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            if (lobbyTimerText != null)
            {
                lobbyTimerText.text = "Time " + $"{minutes:00}:{seconds:00}";
            }

            else
            {
                Debug.LogWarning("[LobbyManager] lobbyTimerText is Null.");
            }
            yield return null;
            timeRemaining -= Time.deltaTime;

        }
        
        Debug.Log("[LobbyManager] Countdown finished.");

        // Inject bots only if we're the server and there's room
        if (!botInjectionTriggered)
        {

            if (humanPlayer.Count == 0 && lobbyPlayers.Any(p => p.isBot && p.IsReady))
            {
                Debug.Log("[LobbyManager] No human players. Injecting bots to start match.");
                InjectBotsAndStart();
                //here is delay for bot injection
                yield return new WaitForSeconds(2f); // Brief pause before injecting bots
            }
            else if (humanPlayer.Count == 0 || allHumanPlayerReady())
            {
                Debug.Log("[LobbyManager] All human players ready or no humans. Injecting bots to fill slots.");
                InjectBotsAndStart();
                yield return new WaitForSeconds(2f); // Brief pause before injecting bots        
            }
            else
            {
                Debug.Log("[LobbyManager] Not all human players ready. Waiting indefinitely for readiness...");
                yield return new WaitUntil(() => allHumanPlayerReady());
                InjectBotsAndStart();
            }

        }
        // Final guard before loading scene
        yield return new WaitUntil(() => NetworkBootstrapper.Runner != null && NetworkBootstrapper.Runner.IsRunning);
        Debug.Log("[LobbyManager] Runner is live. Loading GreyBox...");
        
        var greyBoxScene = SceneRef.FromIndex(3);
        FusionCallbackHandler.SetSceneIndex(greyBoxScene.AsIndex);
        NetworkBootstrapper.Runner.LoadScene(greyBoxScene);
    }

    private IEnumerator WaitForRunnerReadyAndLoadScene()
    {
        while (NetworkBootstrapper.Runner == null || !NetworkBootstrapper.Runner.IsRunning)
            yield return null;

        Debug.Log("[LobbyManager] Runner ready. Loading GreyBox...");
        
        var greyBoxScene = SceneRef.FromIndex(3);
        FusionCallbackHandler.SetSceneIndex(greyBoxScene.AsIndex);
        NetworkBootstrapper.Runner.LoadScene(greyBoxScene);

        Debug.Log("[LobbyManager] Scene load coroutine started");
    }
    private IEnumerator WaitForBotsThenLoadScene()
    {
        // Since lobby bots are UI-only, don’t wait on aiPlayer.Object.IsValid
        yield return new WaitForSeconds(0.5f);

        if (NetworkBootstrapper.Runner != null && NetworkBootstrapper.Runner.IsRunning)
        {
            Debug.Log("[LobbyManager] Proceeding to GreyBox...");
            var greyBoxScene = SceneRef.FromIndex(3);
            FusionCallbackHandler.SetSceneIndex(greyBoxScene.AsIndex);
            NetworkBootstrapper.Runner.LoadScene(greyBoxScene);
        }
        else
        {
            Debug.LogError("[LobbyManager] Runner not running. Aborting scene load.");
        }

    }

    public void InjectBots(int botCount)
    {
        for (int i = 0; i < botCount; i++)
        {
            int team = AssignTeamForPlayer();

            var botInfo = new PlayerInfo(
                name: $"Bot_{i + 1}",
                id: UnityEngine.Random.Range(100000, 999999),
                teamID: team,
                isBot: true,
                isReady: true,
                playerRef: default,
                playFabId: "BOT",
                sessionTicket: "BOT"
            );

            AddPlayerCard(botInfo);
            Debug.Log($"[LobbyManager] Injected bot: {botInfo.PlayerName} on team {team}");
        }
    }




    private void InjectBotsAndStart()
    {
        if (botInjectionTriggered || !NetworkBootstrapper.Runner.IsServer) 
            return;

        int humanCount = lobbyPlayers.Count(p => !p.isBot);
        int botsToAdd = maxPlayer - humanCount;

        if (botsToAdd <= 0)
        {
            Debug.Log($"[Lobby] Injecting {botsToAdd} bots.");
            return;
        }


       botInjectionTriggered = true;

        for (int i = 0; i < botsToAdd; i++)
        {
            InjectBotPlayer(i);
        }

        Debug.Log($"[BotInjection] Calculated botsToAdd: {botsToAdd} (maxPlayer: {maxPlayer}, humanPlayer.Count: {humanPlayer.Count})");
        StartCoroutine(WaitForBotsThenLoadScene());
    }
    private void AssignBotName(PlayerInfo info, int index)
    {
        if (info.isBot && string.IsNullOrWhiteSpace(info.PlayerName))
        { 
            info.PlayerName = $"Bot_{BotGeneratorName.generateName()}";
        }
    }
    private void TryInjectBotsBeforeCountdown()
    {
        if (botInjectionTriggered || !NetworkBootstrapper.Runner.IsServer) return;

        int botsToAdd = maxPlayer - totalPlayers;
        if (botsToAdd <= 0) return;

        botInjectionTriggered = true;
        Debug.Log($"[LobbyManager] Injecting {botsToAdd} bots BEFORE countdown.");

        for (int i = 0; i < botsToAdd; i++)
        {
            InjectBotPlayer(i);
        }
    }



    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

        Debug.Log($"[LobbyManager] OnPlayerJoined called for {player}, but injection is handled by FusionCallbackHandler.");
    }


    private LobbyPlayerCardUI ChooseFreeCardForTeam(int teamId)
    {
        var teamCards = staticCards
            .Where(c => IsUnderTeamPanel(c.transform, teamId))
            .OrderBy(c => c.transform.GetSiblingIndex());

        foreach (var c in teamCards)
        {
            if (!_cardOccupant.ContainsKey(c))
                return c;
        }
        return null;
    }

    private int GetTeamIdForCard(LobbyPlayerCardUI card)
    {
        foreach (var kvp in teamPanelLookup)
        {
            if (IsDescendantOf(card.transform, kvp.Value))
                return kvp.Key;
        }
        // Default: 0 if not found
        return 0;
    }

    private int GetSlotIndexWithinTeam(LobbyPlayerCardUI card, int teamId)
    {
        var parent = GetTeamParent(teamId);
        if (parent == null) return 0;

        // Build ordered list of that team's cards to compute stable per-team index
        var ordered = staticCards
            .Where(c => IsDescendantOf(c.transform, parent))
            .OrderBy(c => c.transform.GetSiblingIndex())
            .ToList();

        int idx = ordered.IndexOf(card);
        return Mathf.Max(0, idx);
    }

    private RectTransform GetTeamParent(int teamId)
    {
        return teamPanelLookup != null && teamPanelLookup.TryGetValue(teamId, out var parent) ? parent : null;
    }

    private bool IsUnderTeamPanel(Transform t, int teamId)
    {
        var parent = GetTeamParent(teamId);
        return parent != null && IsDescendantOf(t, parent);
    }

    private bool IsDescendantOf(Transform child, Transform ancestor)
    {
        if (child == null || ancestor == null) return false;
        var t = child;
        while (t != null)
        {
            if (t == ancestor) return true;
            t = t.parent;
        }
        return false;
    }

    private void OnDestroy()
    {
       Debug.Log("[LobbyManager] OnDestroy called - all coroutines on this object are killed!S");
        
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;  
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GreyBox")
        {
            Debug.Log("[LobbyManager] GreyBox loaded. Cleaning up lobby...");
            Destroy(gameObject); // Clean up lobby manager after scene transition
        }
        
    }
}


