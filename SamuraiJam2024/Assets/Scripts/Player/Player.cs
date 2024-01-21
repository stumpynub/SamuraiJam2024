
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem; 

public enum CharacterState
{
    Idle,
    Grounded,
    GroundedCrouch, 
    Air, 
}

public class Player : MonoBehaviour
{

    const float MaxSpeed = 15f;
    const float Gravity = 40f;

    // private serialized 
    [SerializeField]
    private SettingsPlayer _settings; 

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private GameObject _arms; 

    // private members
    private CharacterController _characterController; 
    private CharacterState _characterState = CharacterState.Grounded;
    private float _cameraRoll = 0;
    private PlayerInput _input;
    private Transform _camInitTransfrom;
    private float _camInitFov = 0; 
    private float _currentFriction = 0;
    private bool _crouched = false; 
    private Vector3 _prevGroundVelocity = Vector3.zero;
    private Vector3 _prevAirVelocity = Vector3.zero;

    private Vector3 _cameraRot = new(); 
    // Input 
    private InputAction _fire; 


    // public members

    public Vector3 Velocity = Vector3.zero;

    [Header("Ground Settings")]
    public float WalkFriction = 20; 
    public float CrouchFriction = 10; 
    public float WalkSpeed = 200;
    public float JumpStrength = 15;

    [Header("Air Settings")]
    public float AirStrafeAmount = 15;

    [Header("Camera Settings")]
    public float CameraRollAmount = 0.2f;


    private void Awake()
    {
        _input = new PlayerInput();
    }

    private void OnEnable()
    {
        _input.Enable();
        _fire = _input.Player.Fire;
        _fire.Enable();
        _fire.performed += Fire; 
    }

    private void OnDisable()
    {
        _input.Disable();
        _fire.Disable(); 
    }

    // Start is called before the first frame update
    void Start()
    {
        _camInitTransfrom = _camera.transform;
        _camInitFov = _camera.fieldOfView; 

        _characterController = GetComponent<CharacterController>();
        _characterController.center = new Vector3(0, _characterController.height / 2, 0); 
    }


    private void FixedUpdate()
    {
        _arms.transform.localEulerAngles += new Vector3(GetLookAxis().y, -GetLookAxis().x, 0) * Time.fixedDeltaTime * 20;
        _arms.transform.localRotation = Quaternion.Slerp(_arms.transform.localRotation, Quaternion.identity, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {

        switch (GetCharacterState())
        {
            case CharacterState.Grounded:
                MoveGroundWalk();
                break; 
            case CharacterState.GroundedCrouch:
                MoveCrouch(); 
                break; 
            case CharacterState.Air:
                MoveAir(); 
                break; 
        }

        if (Velocity.magnitude > 5)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, 85, Time.deltaTime * Velocity.magnitude / 2); 
        } else
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _camInitFov, Time.deltaTime * 2); 
        }

        HandleCameraRotation();

        _characterController.Move((Velocity * Time.deltaTime));

        // snap to ground 
        if (IsOnFloor() && Velocity.y < 1)
        {
            transform.position = Vector3.Lerp(transform.position, GetGroundRay().point, 0.1f);
        }
    }


    void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("callback");
        MeleeAttack(); 
    }
    void MoveCrouch()
    {
        float crouchSlideThreshold = 1.2f; 
        if (!_crouched && Velocity.magnitude <= MaxSpeed * crouchSlideThreshold)
        {
            Velocity = new Vector3(_prevGroundVelocity.x, 0, _prevGroundVelocity.z) * 3;
            _crouched = true;
        };

        if (GetJumpInput()) Velocity = new Vector3(_prevGroundVelocity.x, JumpStrength, _prevGroundVelocity.z);

        Vector3 pos = _camera.transform.localPosition;
        _camera.transform.localPosition = Vector3.Lerp(pos, new Vector3(pos.x, .5f, pos.z), Time.deltaTime * 10);
        _currentFriction = CrouchFriction;
        _crouched = true;

        Vector3 groundNormal = GetGroundRay().normal;
        Accelerate(new Vector3(groundNormal.x * 0.5f, 0, groundNormal.z * 0.5f));

        if (GetGroundAngle() >= _characterController.slopeLimit)
        {
            _currentFriction = 0; 
        }

        ApplyDampening(new Vector3(0, 0, 0), _currentFriction);

        _prevGroundVelocity = Velocity; 
    }

    void MoveGroundWalk()
    {
        Accelerate(GetMoveDirection().normalized * Time.deltaTime * WalkSpeed);
        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);

        if (GetMoveAxis().magnitude <= 0) ApplyDampening(new Vector3(0, 0, 0), _currentFriction);
        if (GetJumpInput()) Velocity = new Vector3(_prevGroundVelocity.x, JumpStrength, _prevGroundVelocity.z);


        if (_crouched) _crouched = false;

        Vector3 pos = _camera.transform.localPosition;
        _camera.transform.localPosition = Vector3.Lerp(pos, new Vector3(pos.x, 1.5f, pos.z), Time.deltaTime * 10);
        _currentFriction = WalkFriction;

        if (GetGroundAngle() > _characterController.slopeLimit)
        {
            Vector3 groundNormal = GetGroundRay().normal;
            Accelerate(new Vector3(groundNormal.x * 0.5f, 0, groundNormal.z * 0.5f));
            _currentFriction = 0; 
        }
        _prevGroundVelocity = Velocity;
    }
    
    void MeleeAttack()
    {
        RaycastHit[] hits = Physics.SphereCastAll(_camera.transform.position, 5, _camera.transform.forward, 1.5f); 

        if (hits.Length > 0)
        {
           foreach (RaycastHit hit in hits)
            {
                if (hit.collider.tag == "Enemy")
                {
                    Debug.Log("Enemy callback"); 
                    AIEnemy enemy = hit.collider.gameObject.GetComponent<AIEnemy>();  ;

                    enemy.Velocity = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up) * 50; 
                }
            }
        }
    }

    void MoveAir()
    {
        Accelerate(GetMoveDirection().normalized * Time.deltaTime * AirStrafeAmount);
        Velocity.y -= Time.deltaTime * Gravity;
    }

    void Accelerate(Vector3 vel)
    {
        Velocity += vel;
    }
    void ApplyDampening(Vector3 damp, float amount) { 
        Velocity = Vector3.Lerp(Velocity, damp, Time.deltaTime * amount);
    }

    void HandleCameraRotation()
    {
        var camRollDot = Vector3.Dot(_camera.transform.right, Velocity) * Mathf.Abs(GetMoveAxis().x);
        var localEulers = _camera.transform.localEulerAngles;

        _cameraRot += new Vector3(-GetLookAxis().y, GetLookAxis().x, 0);
        
        _cameraRot = new Vector3(
            Mathf.Clamp(_cameraRot.x, -89, 89), 
            _cameraRot.y, 
            _cameraRot.z
        ); 

        _cameraRoll = Mathf.Lerp(_cameraRoll, camRollDot, Velocity.magnitude * Time.deltaTime);
        _cameraRoll = Mathf.Lerp(_cameraRoll, 0, Time.deltaTime * 10);

        _camera.transform.rotation = Quaternion.Euler( new Vector3(_cameraRot.x, _cameraRot.y, -_cameraRoll)); 
    }

    public CharacterState GetCharacterState()
    {
        if (IsOnFloor() && !GetCrouchInput())
        {
            return CharacterState.Grounded;
        }

        if (IsOnFloor() && GetCrouchInput())
        {
            return CharacterState.GroundedCrouch;
        }

        if (!IsOnFloor())
        {
           return CharacterState.Air; 
        }

        return CharacterState.Idle; 
    }

    public bool IsOnFloor()
    {
        return GetGroundRay().collider != null; 
    }

    public Vector3 GetMoveDirection()
    {
        Vector3 dir = (_camera.transform.right * GetMoveAxis().x) + (_camera.transform.forward * GetMoveAxis().y);
        dir = Vector3.ProjectOnPlane(dir, Vector3.up);
        return dir; 
    }

    public RaycastHit GetGroundRay()
    {
        RaycastHit spherHit = new RaycastHit();
        RaycastHit rayHit = new RaycastHit();

        Vector3 center = _characterController.center;
        _characterController.center = new Vector3(center.x, 1.3f, center.z); 

        if (Physics.SphereCast(transform.position + _characterController.center, _characterController.radius, Vector3.down, out spherHit, _characterController.center.y + 0.2f))
        {
            if (Physics.Raycast(transform.position + _characterController.center, Vector3.down, out rayHit, 10))
            {
                return rayHit; 
            }
        }

        return new RaycastHit(); 
    }

    public float GetGroundAngle()
    {
        return Vector3.Angle(GetGroundRay().normal, Vector3.up); 
    }

    private Vector2 GetMoveAxis()
    {
        return _input.Player.Move.ReadValue<Vector2>();  
    }

    private Vector2 GetLookAxis()
    {
        float sensitivity = 1; 
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (_settings != null) sensitivity = _settings.MouseSensitivity; 

        return new Vector2(mouseX * sensitivity, mouseY * sensitivity);
    }

    private bool GetJumpInput()
    {
        return _input.Player.Jump.ReadValue<float>() >= 1.0; 
    }

    private bool GetCrouchInput()
    {
        return _input.Player.Crouch.ReadValue<float>() >= 1.0; 
    }
}
