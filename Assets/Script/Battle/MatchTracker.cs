using UnityEngine;
using Cysharp.Threading.Tasks;

// Singleton quản lý trạng thái 1 trận đấu.
// Gắn vào 1 GameObject trong BattleScene.
public class MatchTracker : MonoBehaviour
{
    public static MatchTracker Instance { get; private set; }

    [Header("Match Config")]
    public int maxEnemiesReachEnd = 10;

    public event System.Action<WinData> OnWin;
    public event System.Action<LoseData> OnLose;
    public event System.Action OnNetworkLost;

    private int _enemiesReachedEnd;
    private int _totalAliveEnemies;
    private bool _matchEnded;
    private bool _wasOnline = true;
    private Map _currentMap;

    public struct WinData
    {
        public int gemReward;
        public string mapName;
        public int enemiesReachedEnd;
    }

    public struct LoseData
    {
        public int enemiesReached;
        public int maxEnemies;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void StartMatch(Map map)
    {
        _currentMap = map;
        _enemiesReachedEnd = 0;
        _totalAliveEnemies = 0;
        _matchEnded = false;
    }

    // ── Network monitoring ──

    private void Update()
    {
        if (_matchEnded) return;

        bool online = Application.internetReachability != NetworkReachability.NotReachable;
        if (_wasOnline && !online)
        {
            _wasOnline = false;
            OnNetworkLost?.Invoke();
        }
        else if (!_wasOnline && online)
        {
            _wasOnline = true;
        }
    }

    // ── Enemy tracking ──

    public void RegisterEnemySpawned()  => _totalAliveEnemies++;

    public void RegisterEnemyDied()
    {
        if (_matchEnded) return;
        _totalAliveEnemies--;
        if (_totalAliveEnemies <= 0 && _enemiesReachedEnd < maxEnemiesReachEnd)
            TriggerWin();
    }

    public void RegisterEnemyReachedEnd()
    {
        if (_matchEnded) return;
        _enemiesReachedEnd++;
        _totalAliveEnemies--;

        // Trả enemy về pool
        // (EnemyMoveController gọi hàm này rồi tự Return)

        if (_enemiesReachedEnd >= maxEnemiesReachEnd)
            TriggerLose();
    }

    // ── Win / Lose ──

    private void TriggerWin()
    {
        if (_matchEnded) return;
        _matchEnded = true;
        SaveWinAsync().Forget();
        OnWin?.Invoke(new WinData
        {
            gemReward        = _currentMap != null ? _currentMap.rewardWinMap : 0,
            mapName          = _currentMap != null ? _currentMap.mapName : "",
            enemiesReachedEnd = _enemiesReachedEnd
        });
    }

    private void TriggerLose()
    {
        if (_matchEnded) return;
        _matchEnded = true;
        OnLose?.Invoke(new LoseData
        {
            enemiesReached = _enemiesReachedEnd,
            maxEnemies     = maxEnemiesReachEnd
        });
    }

    // Gọi khi player chủ động thoát trận
    public void ForceLose() => TriggerLose();

    // Chỉ lưu khi thắng
    private async UniTaskVoid SaveWinAsync()
    {
        var pdm = PlayerDataManager.Instance;
        if (pdm == null || _currentMap == null) return;
        await pdm.CompleteMapAsync(_currentMap.id).AsUniTask();
    }

    // ── Thoát app giữa trận ──

    private void OnApplicationPause(bool pause)
    {
        if (pause && !_matchEnded)
            HandleAppQuit();
    }

    private void OnApplicationQuit()
    {
        if (!_matchEnded)
            HandleAppQuit();
    }

    private void HandleAppQuit()
    {
        // Không lưu progress, không cộng reward — chỉ đảm bảo không ghi sai data
        _matchEnded = true;
        Debug.Log("[MatchTracker] App thoát giữa trận — không lưu kết quả.");
    }
}
