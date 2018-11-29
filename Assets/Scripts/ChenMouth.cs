using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChenMouth : MonoBehaviour {

    Animator m_animator;

    private string m_animation_parameter = "CurrentState";
    private int m_animation_idle = 0;
    private int m_animation_speak = 1;

    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public void invokeSpeak()
    {
        if (m_animator != null)
        {
            m_animator.SetInteger(m_animation_parameter, m_animation_speak);
        }
    }


    public void invokeIdle()
    {
        if (m_animator != null)
        {
            m_animator.SetInteger(m_animation_parameter, m_animation_idle);
        }
    }
}
