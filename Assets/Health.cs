using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public GameObject[] healthBar;
    private ParticleSystem _particleSystem;
    public float hitpoints = 100;
    private float _lastHitpoints;
    private float _startHitpoints;
    public float manapoints = 100;
    private float _onFire;
    public Action DestroyCallback;

    void Start()
    {
        foreach (var h in healthBar)
        {
            h.GetComponent<Renderer>().material.color = Color.red;
        }
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _startHitpoints = hitpoints;
        _onFire = Math.Max(_startHitpoints * .5f, 200f);
    }

    void Update()
    {
        if (hitpoints <= 0)
        {
            Destroy(gameObject);
        } 
        else if (hitpoints <= _onFire && _particleSystem && !_particleSystem.isPlaying)
        {
            _particleSystem.Play();
        }

        if (Math.Abs(hitpoints - _lastHitpoints) > 1f)
        {
            UpdateHealthBar();
        }
        _lastHitpoints = hitpoints;
    }

    private void UpdateHealthBar()
    {
        var pct = hitpoints / _startHitpoints;
        var boxes = healthBar.Length - (int) (pct * healthBar.Length);
        for (var i = 0; i < healthBar.Length; i++)
        {
            healthBar[i].SetActive(i >= boxes);
        }
    }

    public float ChangeHitpints(float changeAmount)
    {
        var newHp = hitpoints + changeAmount;
        if (newHp > _startHitpoints)
        {
            return 0f;
        }
        _lastHitpoints = hitpoints = newHp;
        UpdateHealthBar();
        return changeAmount;
    }

    void OnDestroy()
    {
        DestroyCallback?.Invoke();
    }
}
