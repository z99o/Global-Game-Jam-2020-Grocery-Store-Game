using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Movement_3D : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Movement")]
    [SerializeField] private float m_base_speed = 0f;
    [SerializeField] public float  m_cur_max_speed;
    [SerializeField] public float  m_walk_max_speed;
    [SerializeField] public float  m_cur_speed_x;
    [SerializeField] public float  m_cur_speed_z;
    [SerializeField] public float m_acceleration_rate;
    [SerializeField] public float m_jump_height;
    [SerializeField] private float m_gravity = -9.8f;
    [SerializeField] private float m_sprint_multiplier;
    [SerializeField] public float m_sprint_cur_level;// { get; private set; }
    [SerializeField] public float m_sprint_max_level; //{ get; private set; }
    [SerializeField] private float m_sprint_recovery_speed;
    [SerializeField] public bool m_sprint_is_exhausted;
    [SerializeField] private float m_max_sprint_speed;
    [SerializeField] public float m_health;
    [SerializeField] public float m_max_health;
    [SerializeField] public float m_isMoving;
    [SerializeField] public AudioClip footStep;
    [SerializeField] public float volume_min;
    [SerializeField] public float volume_max;
    [SerializeField] public float stepDistance;
    private float accumulatedDistance = 0;

    [SerializeField] Vector3 m_velocity;
    [SerializeField] public bool m_is_grounded;
    [SerializeField] public float grounded_clearance; //How large the grounded check sphere is

    [Header("Actions")]
    [SerializeField] public float kick_strength;
    [SerializeField] public float kick_duration = 0.25f;
    [SerializeField] public float kick_timer = 0;

    [Header("Input")]
    [SerializeField] public float i_x_movement;
    [SerializeField] public float i_z_movement;
    [SerializeField] public bool i_is_jumping;
    [SerializeField] public bool i_is_kicking;
    [SerializeField] public bool i_is_sprinting;
    [Header("References")]
    [SerializeField] public Transform leg_joint;
    [SerializeField] public CharacterController controller;
    [SerializeField] public Transform cam;
    [SerializeField] public LayerMask ground_mask;
    [SerializeField] public Transform ground_check;

    [HideInInspector]public Vector3 kickDirection;
    [HideInInspector]public float kickDuration;
    
    void Start()
    {
        kickDirection = Vector3.zero;
    }

    void Update()
    {
        m_is_grounded = Check_If_Grounded();
        Get_Inputs();
        Move();
        checkToPlaySteps();

        if(kickDuration > 0) {
            GetComponent<CharacterController>().Move(kickDirection * Time.deltaTime);
            kickDuration -= Time.deltaTime;
        }

    }

    public bool Check_If_Grounded(){
        return Physics.CheckSphere(ground_check.position,grounded_clearance,ground_mask);
    }

    void Move(){
        //Get our inputs
        float x = i_x_movement;
        float z = i_z_movement;
        Vector2 speed = Calculate_Speed(x,z);
        speed *= Calculate_Sprint();
        //Multiply by speed components
        Vector3 move = transform.right * speed.x + transform.forward * speed.y;
        m_isMoving = move.magnitude;
        //m_velocity = (move * Time.deltaTime);
        controller.Move(move * Time.deltaTime);
        //Compute jump
        Jump();
        Apply_Gravity();
        controller.Move(m_velocity*Time.deltaTime);
    }

    private void Apply_Gravity(){
        m_velocity.y += m_gravity * Time.deltaTime;
    }

    private void Jump(){
        if(m_is_grounded && m_velocity.y < 0)
            m_velocity.y = -2;

        if(i_is_jumping && m_is_grounded){
            m_velocity.y = Mathf.Sqrt(m_jump_height * -2 * m_gravity);
        }
    }

    private void Get_Inputs(){
        i_x_movement = Input.GetAxis("Horizontal");
        i_z_movement = Input.GetAxis("Vertical");
        i_is_jumping = Input.GetButtonDown("Jump");
        i_is_sprinting = Input.GetButton("Sprint");
        i_is_kicking = Input.GetButtonDown("Kick");
    }
    public Vector2 Calculate_Speed(float x, float z){
        if(x != 0 || z != 0){
            //look at last velocity and apply acceleration based off our movements
            m_cur_speed_x += x * Time.deltaTime * m_acceleration_rate;
            m_cur_speed_z += z * Time.deltaTime * m_acceleration_rate;
        }
        //Axis inputs will still be non zero even when the key is not depressed so we evaluate between -1 and 1
        //decreasing the value (absolutely) will allow for a spotting distance
        float val = 0.5f;
        if(x < val && x > -val){
            //reset x's acceleration
            m_cur_speed_x = m_base_speed;
        }
        if(z < val && z > -val){
            //reset z's acceleration
            m_cur_speed_z = m_base_speed;
        }
        //Lastly, clamp everything
        m_cur_speed_x = Mathf.Clamp(m_cur_speed_x,-m_walk_max_speed,m_walk_max_speed);
        m_cur_speed_z = Mathf.Clamp(m_cur_speed_z,-m_walk_max_speed,m_walk_max_speed);
        return new Vector2(m_cur_speed_x,m_cur_speed_z);
    }

    public float Calculate_Sprint() {
        // undo any exaustion
        if (m_sprint_cur_level > m_sprint_max_level)
            m_sprint_is_exhausted = false;

        if (m_sprint_cur_level < 0)
            m_sprint_is_exhausted = true;

        m_sprint_cur_level = Mathf.Clamp(m_sprint_cur_level, 0, m_sprint_max_level);
        if (i_is_sprinting) {
            if (m_sprint_is_exhausted) {
                //recover
                m_sprint_cur_level += Time.deltaTime * m_sprint_recovery_speed;
                return 0.5f;
            }
            //expend
            m_cur_max_speed = m_max_sprint_speed;
            m_sprint_cur_level -= Time.deltaTime * 2 * m_sprint_recovery_speed;
            return m_sprint_multiplier;
        }
        m_cur_max_speed = m_walk_max_speed;
        //recover
        m_sprint_cur_level += Time.deltaTime * m_sprint_recovery_speed;
        if (m_sprint_is_exhausted) {
            return 0.5f;
        }
        return 1;

    }

    public void Take_Damage(float damage) {
        m_health -= damage;
        if(m_health <= 0) {
            //gameover
        }
    }

    public void checkToPlaySteps() {
        if (!m_is_grounded)
            return;

        if (m_isMoving != 0) {
            float stepRate = stepDistance / m_isMoving;
            accumulatedDistance += Time.deltaTime;

            if(accumulatedDistance > stepRate) {

                float volume = Random.Range(volume_min, volume_max);
                GetComponent<AudioSource>().PlayOneShot(footStep, volume);
                accumulatedDistance = 0f;
            }
        }
    }

}
