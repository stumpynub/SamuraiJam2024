using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSword : MonoBehaviour
{

    public GameObject HandTarget;
    public GameObject IkTarget; 
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            int clipsLength = clips.Length;
            int clipIndex = Mathf.Abs(Random.Range(0, clipsLength));
            animator.Play(clips[clipIndex].name);
        }

        HandTarget.transform.position = IkTarget.transform.position;
        HandTarget.transform.rotation = IkTarget.transform.rotation;
    }
}
