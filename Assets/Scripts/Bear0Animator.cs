using UnityEngine;

public class Bear0Animator : MonoBehaviour
{
    public Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        //anim.SetFloat("Speed", 3.0f);
    }

    // how to set animations based on triggers from domain execution:
    // npc.GetComponent<Animator>()?.SetTrigger("Sleep");

}
