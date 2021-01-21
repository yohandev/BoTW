using UnityEngine;

/// <summary>
/// gravity for game objects with character controllers
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Gravity : MonoBehaviour
{
    public Vector3 force = Vector3.down * 9.81f;

    private CharacterController _controller;
    
    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        _controller.SimpleMove(force * Time.deltaTime);
    }
}
