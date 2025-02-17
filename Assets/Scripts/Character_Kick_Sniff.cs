using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character_Kick_Sniff : MonoBehaviour {

    [Header("Sniffing Settings")]
    public long lastSniff = 0;
    public float sniffCooldownMs = 7000; //in millisconds 

    public float maxIndicatorAlpha; //Max value of alpha
    public float distMult; //How far away you're able to get max alpha
    public AnimationCurve m_tintCurve;
    public float m_curveDuration;
    public Transform tintObject;
    public AudioClip[] sniffSounds;

    [Header("Kicking Settings")]
    public Transform kickOrigin;
    public float kickRadius;
    public float kickForce;
    public AudioClip kickSound;

    private GameObject cereal;

    // Start is called before the first frame update
    void Start() {
        cereal = GameObject.Find("Cereal");
    }

    // Update is called once per frame
    void Update() {
        if(cereal == null){
            cereal = GameObject.Find("Cereal");
        }
        kick_timer+= Time.deltaTime;
        if (Input.GetButtonDown("Kick")) {
            Kick();
        }

        if (Input.GetButtonDown("Sniff")) {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (now - lastSniff > sniffCooldownMs)
            {
                Sniff();
                lastSniff = now;
            }
        }
    }

    public float sniff_volume;
    void Sniff() {
        float dist = Vector3.Distance(transform.position, cereal.transform.position);
        float alpha = (maxIndicatorAlpha/dist) * distMult;
        alpha = Mathf.Clamp(alpha, 0, maxIndicatorAlpha);

        int sniffVal = (int)(Mathf.Round(UnityEngine.Random.value * 3));
        GetComponent<AudioSource>().PlayOneShot(sniffSounds[sniffVal], sniff_volume);

        StartCoroutine(EvalCurve(alpha, tintObject));
    }

    public float kick_volume;
    public float kick_cooldown = 1.5f;
    public float kick_timer;
    void Kick() {
        if(kick_timer < kick_cooldown)
            return;
        Collider[] colliders = Physics.OverlapSphere(kickOrigin.position, kickRadius);

        foreach (Collider nearbyObject in colliders) {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.AddForce(Camera.main.transform.forward * kickForce);
                GetComponent<AudioSource>().PlayOneShot(kickSound, kick_volume); //ow my ears
            }
        }
        kick_timer = 0;
    }

    IEnumerator EvalCurve(float alpha, Transform tintObject) {
        float time = 0;
        while (time <= m_curveDuration) {
            float value = m_tintCurve.Evaluate(time/m_curveDuration);
            time += Time.deltaTime;

            Color tint = new Color(0, 1, 0, value * (alpha / 255));
            tintObject.GetComponent<Image>().color = tint;
            yield return null;
        }
    }
}
