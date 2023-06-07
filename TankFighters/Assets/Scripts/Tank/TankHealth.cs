﻿using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.Networking;


public class TankHealth : NetworkBehaviour
{
    public float m_StartingHealth = 100f;          
    public Slider m_Slider;                        
    public Image m_FillImage;                      
    public Color m_FullHealthColor = Color.green;  
    public Color m_ZeroHealthColor = Color.red;    
    public GameObject m_ExplosionPrefab;
    

    private AudioSource m_ExplosionAudio;          
    private ParticleSystem m_ExplosionParticles;

    private NetworkVariable<float> m_CurrentHealth = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> m_Dead = new NetworkVariable<bool>();
    //public float m_CurrentHealth;  
    //private bool m_Dead;  


    private void Awake()
    {
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        m_ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {

        m_CurrentHealth.Value = m_StartingHealth;
        m_Dead.Value = false;

        SetHealthUIServerRpc();
    }
    
    //[ServerRpc]
    //private void SetStartingHealthServerRpc()
    //{
    //    m_CurrentHealth = m_StartingHealth;
    //    m_Dead = false;
    //}


    [ServerRpc (RequireOwnership = false)]
    public void TakeDamageServerRpc(float amount)
    {

        // Adjust the tank's current health, update the UI based on the new health and check whether or not the tank is dead.
        m_CurrentHealth.Value -= amount;

        SetHealthUIServerRpc();

        if (m_CurrentHealth.Value <= 0f && !m_Dead.Value)
        {
            OnDeathServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthUIServerRpc()
    {

        // Adjust the value and colour of the slider.
        m_Slider.value = m_CurrentHealth.Value;
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth.Value / m_StartingHealth);

    }

    [ServerRpc(RequireOwnership = false)]
    private void OnDeathServerRpc()
    {
        // Play the effects for the death of the tank and deactivate it.
        m_Dead.Value = true;

        m_ExplosionParticles.GetComponent<NetworkObject>().Spawn();
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        m_ExplosionParticles.Play();
        m_ExplosionAudio.Play();
        gameObject.SetActive(false);
    }
}