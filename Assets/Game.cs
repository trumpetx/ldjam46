using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(AudioSource))]
public class Game : MonoBehaviour
{
    public struct Level
    {
        public float hutHp;
        public float spawnChance;
        public float bigGuyChance;
        public float dmgMod;
        public float minionMod;
        public int maxSpawnCt;
        public float speedMod;
        public float spellMod;

        internal Level(float hutHp, float spawnChance, float bigGuyChance, float dmgMod, float minionMod, int maxSpawnCt, float speedMod, float spellMod)
        {
            this.hutHp = hutHp;
            this.spawnChance = spawnChance;
            this.bigGuyChance = bigGuyChance;
            this.dmgMod = dmgMod;
            this.minionMod = minionMod;
            this.maxSpawnCt = maxSpawnCt;
            this.speedMod = speedMod;
            this.spellMod = spellMod;
        }

        public override string ToString()
        {
            return "hutHp: " + hutHp + "\n" +
                   "spawnChance: " + spawnChance + "\n" +
                   "bigGuyChance: " + bigGuyChance + "\n" +
                   "dmgMod: " + dmgMod + "\n" +
                   "minionMod: " + minionMod + "\n" +
                   "maxSpawnCt: " + maxSpawnCt + "\n" +
                   "speedMod: " + speedMod + "\n" +
                   "spellMod: " + spellMod + "\n";
        }
    }

    private bool _gameOver;
    private int _level;
    private Level _player1;
    private const float StartMana = 100f; 
    private float _maxPlayerMana;
    private float _playerMana;
    private float _levelPlayerMana;
    private const float ManaRegen = .334f;
    private readonly Level[] _Levels =
    {
        new Level(900, .05f, .05f, 1f, .9f, 5, 1, 1), 
        new Level(1100, .075f, .05f, 1f, 1f, 5, 1, 1), 
        new Level(1300, .075f, .07f, 1f, 1f, 5, 1.1f,1 ), 
        new Level(1500, .1f, .08f, 1f, 1.1f, 5, 1.1f, 1), 
        new Level(1700, .1f, .1f, 1.05f, 1.1f, 6, 1.2f, 1), 
        new Level(1900, .11f, .1f, 1.1f, 1.2f, 6, 1.2f, 1), 
        new Level(2100, .11f, .1f, 1.2f,1.3f, 7, 1.3f, 1), 
        new Level(2500, .12f, .15f, 1.1f, 1.4f, 7, 1.3f, 1), 
        new Level(2900, .15f, .15f, 1.2f, 1.5f, 8, 1.3f, 1), 
        new Level(3500, .17f, .17f, 1.2f, 1.6f, 8, 1.3f, 1)
    };

    public Text levelText;
    private float _startVol;
    private AudioSource _audioSource;
    public AudioClip winMusic;
    public AudioClip fightMusic;
    public AudioClip outOfMana;
    public GameObject buttonPane;
    public Button healButton;
    public HealthOverTime healOverTime;
    public Button fireButton;
    public HealthOverTime fireOverTime;
    public Button summonButton;
    public GameObject newGamePane;
    public GameObject objective;
    public Reinforcements orcs;
    public Reinforcements trolls;
    private Coroutine _audioChange;
    public Image heathBar;
    private Coroutine _manaTickCorutiCoroutine;
    public GameObject overlap;
    public Color groundColor;

    private Level PlayerStartLevel()
    {
        return new Level(2000, .1f, .05f, 1, 1, 5, 1, 1);
    }

    private const int FIRE = 1;
    private const int HEAL = 2;
    private const int SUMMON = 3;
    private int _special;

    private void OnEnable()
    {
        _audioSource = GetComponent<AudioSource>();
        _startVol = _audioSource.volume;
    }

    void Start()
    {
        GetComponent<MeshRenderer>().material.color = groundColor;
        overlap.GetComponent<MeshRenderer>().material.color = groundColor;
        _playerMana = _levelPlayerMana = _maxPlayerMana = StartMana;
        _level = 0;
        _player1 = PlayerStartLevel();
        ResetGame( );
        healButton.onClick.RemoveAllListeners();
        healButton.onClick.AddListener(Heal);
        fireButton.onClick.RemoveAllListeners();
        fireButton.onClick.AddListener(Fire);
        summonButton.onClick.RemoveAllListeners();
        summonButton.onClick.AddListener(Summon);
        if (_manaTickCorutiCoroutine == null)
        {
            _manaTickCorutiCoroutine = StartCoroutine(nameof(ManaTick));
        }
    }
    
    private static void SetTransparency(Image image, float transparency)
    {
        if (!image) return;
        var __alpha = image.color;
        __alpha.a = transparency;
        image.color = __alpha;
    }

    private void ChangeMana(float change)
    {
        _playerMana = Math.Min(_playerMana + change, _maxPlayerMana);
        heathBar.fillAmount = _playerMana / _maxPlayerMana;
    }
    
    private IEnumerator ManaTick() {
        for(;;)
        {
            if (!_gameOver)
            {
                ChangeMana(ManaRegen * _player1.spellMod);
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown (0)) {
            Clicked();
        }
    }
    
    void Clicked()
    {
        if (_gameOver || _special == 0)
        {
            return;
        }

        Action<RaycastHit> action;
        float manaCost;
        switch (_special)
        {
            case FIRE:
                action = h => Aoe(fireOverTime.gameObject, h);
                manaCost = fireOverTime.manaCost;
                break;
            case HEAL:
                action = h => Aoe(healOverTime.gameObject, h);
                manaCost = healOverTime.manaCost;
                break;
            case SUMMON:
                action = h => orcs.Spawn(orcs.spawns[Random.Range(0, orcs.spawns.Length)], h.point + Vector3.up * .5f, true, _player1.spellMod);
                manaCost = fireOverTime.manaCost;
                break;
            default:
                return;
        }
        if (manaCost > _playerMana)
        {
            if (outOfMana)
            {
                _audioSource.PlayOneShot(outOfMana);
            }
            return;
        }
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = new RaycastHit();
        var layerMask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            ChangeMana(-manaCost);
            action.Invoke(hit);
        }
    }

    void Aoe(GameObject toInstntiate, RaycastHit hit)
    {
        var areaEffect = Instantiate(toInstntiate, hit.point, Quaternion.identity);
        areaEffect.GetComponentInChildren<ParticleSystem>().Play();
        Destroy(areaEffect.gameObject, 5);
    }

    void Fire()
    {
        _special = FIRE;
        SetTransparency(summonButton.image, 1);
        SetTransparency(healButton.image, 1);
        SetTransparency(fireButton.image, .5f);
    }
    void Summon()
    {
        _special = SUMMON;
        SetTransparency(summonButton.image, .5f);
        SetTransparency(healButton.image, 1);
        SetTransparency(fireButton.image, 1f);
    }
    void Heal()
    {
        _special = HEAL;
        SetTransparency(summonButton.image, 1);
        SetTransparency(healButton.image, .5f);
        SetTransparency(fireButton.image, 1f);
    }

    void ResetHut(Reinforcements reinforcements, bool win, Game.Level level)
    {
        var hut = Instantiate(objective, reinforcements.hutSpawn.transform.position, Quaternion.identity);
        var hutHealth = hut.GetComponent<Health>();
        hutHealth.DestroyCallback = () => GameOver(win);
        hutHealth.hitpoints = level.hutHp;
        hut.tag = reinforcements.tag;
        hut.GetComponent<Renderer>().material.color = reinforcements.color;
        reinforcements.hut = hut;
        reinforcements.SetLevel(level);
    }

    void ResetGame( )
    {
        _playerMana = _levelPlayerMana;
        levelText.text = ""+(_level + 1);
        Level level = _Levels[_level];
        // Debug.Log("Game reset with level:\n" + level + "\n\nAnd Player Level:\n" + _player1);
        newGamePane.SetActive(false);
        buttonPane.SetActive(true);
        ResetHut(orcs, false, _player1);
        ResetHut(trolls, true, level);
        _gameOver = false;
        _audioSource.clip = fightMusic;
        _audioSource.volume = _startVol;
        if(_audioChange != null) StopCoroutine(_audioChange);
        _audioChange = StartCoroutine(AudioFadeScript.FadeIn(_audioSource, 5f));
    }

    void GameOver(bool win)
    {
        if (_gameOver)
        {
            return;
        }
        _gameOver = true;
        if(_audioChange != null) StopCoroutine(_audioChange);
        _audioChange = StartCoroutine(AudioFadeScript.FadeOut(_audioSource, 2f));
        foreach (var t in new []{"orx", "toll"})
        {
            GameObject.FindGameObjectsWithTag(t).Where(go => !go.GetComponent<Reinforcements>()).ToList().ForEach(Destroy);
        }
        GameObject.FindGameObjectsWithTag("Finish").ToList().ForEach(DestroyImmediate);
        var buttons = newGamePane.GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }
        var text = newGamePane.GetComponentInChildren<Text>();
        if (win)
        {
            _level += 1;
            if ( _level == _Levels.Length)
            {
                _audioSource.Stop();
                _audioSource.clip = winMusic;
                _audioSource.volume = _startVol;
                _audioSource.Play();
                text.text = "YOU HAVE WON!\nYour tribe will live forever!";
                buttons[0].gameObject.SetActive(true);
                buttons[0].onClick.AddListener(Start);
                buttons[0].GetComponentInChildren<Text>().text = "Start Over?";
                // buttons[2].gameObject.SetActive(true);
                // buttons[2].onClick.AddListener(Application.Quit);
                // buttons[2].GetComponentInChildren<Text>().text = "Retire?";
            }
            else
            {
                _levelPlayerMana = _playerMana;
                text.text = "You have recaptured " + (((int) (_level / (float) _Levels.Length * 100))/100f) * 100 + "% of your territory.  How will you prepare for the next battle?";
                buttons[0].GetComponentInChildren<Text>().text = "ONWARD: Attack the next Village - no changes!";
                buttons[0].gameObject.SetActive(true);
                buttons[0].onClick.AddListener(ResetGame);
                List<Action<Button>> powerUps = new List<Action<Button>>(new Action<Button>[]{ Recruit, PowerUp, Enrage, EarlyBird, Rush, Study, Meditate, Fortify })
                    .OrderBy( x => Random.value ).ToList( );
                for (var i = 1; i < buttons.Length; i++)
                {
                    powerUps[i-1].Invoke(buttons[i]);
                }
            }
        }
        else
        {
            text.text = "Retreat!!";
            buttons[0].gameObject.SetActive(true);
            buttons[0].onClick.AddListener(ResetGame);
            buttons[0].GetComponentInChildren<Text>().text = "Try again?";
            buttons[1].gameObject.SetActive(true);
            buttons[1].onClick.AddListener(Start);
            buttons[1].GetComponentInChildren<Text>().text = "Give up and start over?";
        }
        newGamePane.SetActive(true);
        buttonPane.SetActive(false);
    }

    private void Recruit(Button button)
    {
        button.GetComponentInChildren<Text>().text = "RECRUIT: Give up half Hut health for one more Max Minions";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.maxSpawnCt += 2;
            _player1.hutHp *= .5f;
            ResetGame();
        });
    }
    
    private void PowerUp(Button button)
    {
        button.GetComponentInChildren<Text>().text = "POWER-UP: Increase the chance for Big Minions to spawn in exchange for one fewer Max Minions";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.maxSpawnCt -= 1;
            _player1.bigGuyChance *= 2;
            ResetGame();
        });
    }
    
    private void EarlyBird(Button button)
    {
        button.GetComponentInChildren<Text>().text = "EARLY-BIRD: Spawn Minions much faster";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.spawnChance *= 2;
            ResetGame();
        });
    }
    
    private void Rush(Button button)
    {
        button.GetComponentInChildren<Text>().text = "RUSH: Minions are faster, but do less damage";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.dmgMod *= .93f;
            _player1.speedMod += .15f;
            _player1.spawnChance *= 1.1f;
            ResetGame();
        });
    }
    private void Meditate(Button button)
    {
        button.GetComponentInChildren<Text>().text = "MEDITATE: Regenerate all of your Mana and increase Max Mana";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _maxPlayerMana *= 1.1f;
            ChangeMana(_maxPlayerMana);
            _levelPlayerMana = _playerMana;
            ResetGame();
        });
    }
    private void Fortify(Button button)
    {
        button.GetComponentInChildren<Text>().text = "FORTIFY: Increase Hut health by 1000";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.hutHp += 1000;;
            ResetGame();
        });
    }
    private void Study(Button button)
    {
        button.GetComponentInChildren<Text>().text = "SPELL STUDY: Increase the power of your spells and increase Mana regeneration";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.spellMod *= 1.2f;
            ResetGame();
        });
    }
    private void Enrage(Button button)
    {
        button.GetComponentInChildren<Text>().text = "ENRAGE: Increase minion damage and lower minion health";
        button.gameObject.SetActive(true);
        button.onClick.AddListener(() =>
        {
            _player1.minionMod *= .9f;
            _player1.dmgMod *= 1.3f;
            ResetGame();
        });
    }
}
