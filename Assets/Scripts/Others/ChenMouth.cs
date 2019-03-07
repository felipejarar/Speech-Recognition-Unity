using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChenMouth : MonoBehaviour {

    Animator m_animator;

    private string m_animation_parameter = "CurrentState";
    private int m_animation_sleep = 0;
    private int m_animation_awake = 1;
    private int m_animation_speak = 2;
    private int m_animation_dance = 3;

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public void invokeSleep(){ if (m_animator != null) m_animator.SetInteger(m_animation_parameter, m_animation_sleep); }
    public void invokeAwake() { if (m_animator != null) m_animator.SetInteger(m_animation_parameter, m_animation_awake); }
    public void invokeSpeak() { if (m_animator != null) m_animator.SetInteger(m_animation_parameter, m_animation_speak); }
    public void invokeDance() { if (m_animator != null) m_animator.SetInteger(m_animation_parameter, m_animation_dance); }

}
