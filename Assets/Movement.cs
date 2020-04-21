using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Health), typeof(AudioSource))]
public class Movement : MonoBehaviour
{
    public float FootDistance = 1f;
    private Vector3 _startingForward;
    private Vector3 _startingBack;
    public GameObject Hut;
    private GameObject Target;
    public string targetTag;
    public float speed = 5;
    public Transform BackFoot;
    public Transform ForwardFoot;
    public Transform Weapon;
    private ParticleSystem _weaponParticleSystem;
    public float Dmg = 1;
    private bool colliding;
    private Game.Level _level;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        var health = GetComponent<Health>();
        health.hitpoints *= _level.minionMod;
        health.manapoints *= _level.minionMod;
        
        _startingForward = ForwardFoot.position;
        _startingBack = BackFoot.position;
        if (Weapon)
        {
            _weaponParticleSystem = Weapon.GetComponentInChildren<ParticleSystem>();
        }
    }

    public void SetLevel(Game.Level level)
    {
        _level = level;
        speed *= level.speedMod;
        Dmg *= level.dmgMod;
    }

    private void OnTriggerEnter(Collider c)
    {
        if (targetTag != null && c.CompareTag(targetTag) && c.gameObject != Target)
        {
            Target = FindClosestEnemy();
        }
    }

    private void OnTriggerStay(Collider c)
    {
        if (c.gameObject != Target) return;
        colliding = true;
        var other = c.GetComponent<Health>();
        if (other)
        {
            other.hitpoints -= Time.deltaTime * Dmg;
        }
    }

    void Update()
    {
        if (!Target || !Target.gameObject.activeInHierarchy)
        {
            Target = FindClosestEnemy();
            colliding = false;
        }

        if (colliding)
        {
            if (_audioSource && _audioSource.clip && !_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
            if (_weaponParticleSystem && !_weaponParticleSystem.isPlaying)
            {
                _weaponParticleSystem.Play(false);
            }
        }
        else
        {
            if (_weaponParticleSystem && _weaponParticleSystem.isPlaying)
            {
                _weaponParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
        MoveTowardTarget();
    }

    private void MoveTowardTarget()
    {
        if (Target && !colliding)
        {
            var position = transform.position;
            var rotation = Quaternion.LookRotation(position - Target.transform.position);
            var pos = position - transform.forward * (speed * Time.deltaTime);
            pos.y = position.y;
            transform.SetPositionAndRotation(pos, rotation);
            MoveFeet(rotation);
        }
    }

    void MoveFeet(Quaternion Rotation)
    {
        var FootFoward = transform.position + (Rotation * -Vector3.forward * FootDistance);
        var distance = Vector3.Distance(FootFoward, ForwardFoot.position);
        if (distance > (FootDistance * 3))
        {
            // catch the 'runaway' feet
            ForwardFoot.SetPositionAndRotation(_startingForward, Rotation);
            BackFoot.SetPositionAndRotation(_startingBack, Rotation);
        } 
        else if (distance < FootDistance)
        {
            var ft = BackFoot;
            BackFoot = ForwardFoot;
            ForwardFoot = ft;
        }

        var BackPos = BackFoot.position + (Rotation * Vector3.forward) * (speed * Time.deltaTime);
        BackPos.y = BackFoot.position.y;
        BackFoot.SetPositionAndRotation(BackPos, Rotation);
        var ForwardPos = ForwardFoot.position + (Rotation * Vector3.forward) * (-speed * Time.deltaTime);
        ForwardPos.y = ForwardFoot.position.y;
        ForwardFoot.SetPositionAndRotation(ForwardPos, Rotation);
    }
    
    GameObject FindClosestEnemy()
    {
        if (targetTag == null || targetTag.Trim() == "")
        {
            Debug.Log("Target Tag is null");
            return Hut;
        }
        var gos = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        var distance = Mathf.Infinity;
        var position = transform.position;
        foreach (GameObject go in gos)
        {
            if (go.GetComponent<Reinforcements>())
            {
                // Skip the spawn objects
                continue;
            }
            var diff = go.transform.position - position;
            var curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }

        return !closest ? Hut : closest;
    }
}
