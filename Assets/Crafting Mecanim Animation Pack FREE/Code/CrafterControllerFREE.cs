using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CharacterState
{
	Idle,
	Box
};

public class CrafterControllerFREE : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> characterList = new List<GameObject>();

	public GameObject dustParticles;
	public GameObject waterParticles;
	public GameObject groundCheck;

    private Animator animator;
	public Rigidbody rb;
	private GameObject box;
	float rotationSpeed = 5;
	Vector3 inputVec;
	bool isMoving;
	bool isPaused;
	public CharacterState charState;
	public Transform cam;
    public CharacterController controller;
    public float speed;

     public Rigidbody m_Rigidbody;

    public float turnSmoothTime;

    float turnSmoothVelocity;

    void Awake()
	{
		animator = this.GetComponent<Animator>();
		box = GameObject.Find("Carry");
	}

	void Start()
	{
		//StartCoroutine(COShowItem("none", 0f));
		charState = CharacterState.Idle;
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
	}



    private void AnimatorAction(System.Action<Animator> act)
    {
        foreach (var chara in characterList)
        {
            if (chara && chara.activeInHierarchy)
            {
                var animator = chara.GetComponent<Animator>();
                if (animator)
                {
                    act(animator);
                }
            }
        }
    }



    void Update()
	{
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

		if (direction.magnitude > 0f && Input.GetKey(KeyCode.LeftShift))
        {
			float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);

			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
			controller.Move(moveDir.normalized * speed * 1.5f * Time.deltaTime);
			animator.SetBool("Idle", false);
			animator.SetBool("Run", true);
			animator.SetBool("Walk", false);
			//animator.SetTrigger("Run");
		}

		else if (direction.magnitude > 0f && !Input.GetKey(KeyCode.LeftShift))
		{
			float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
			float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
			transform.rotation = Quaternion.Euler(0f, angle, 0f);

			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
			controller.Move(moveDir.normalized * speed * 0.25f * Time.deltaTime);
			animator.SetBool("Idle", false);
			animator.SetBool("Run", false);
			animator.SetBool("Walk", true);
			//animator.SetTrigger("Walk");

			if (groundCheck.GetComponent<CheckGround>().groundType == "dirt")
            {
				//dustParticles.SetActive(true);
			}
            else
            {
				dustParticles.SetActive(false);
			}
			if (groundCheck.GetComponent<CheckGround>().groundType == "water")
            {
				waterParticles.SetActive(true);
			}
            else
			{
				waterParticles.SetActive(false);
			}
		}

		if (direction.magnitude <= 0f)
		{
			animator.SetBool("Walk", false);
			animator.SetBool("Run", false);
			//animator.SetTrigger("Idle");
			dustParticles.SetActive(false);
			if (Input.GetKey(KeyCode.Space))
			{
				animator.SetBool("Idle", false);
				animator.SetBool("Jump", true);
			}
			else
			{
				animator.SetBool("Jump", false);
				animator.SetBool("Idle", true);
			}

		}

		//update character position and facing
		UpdateMovement();

		if(Input.GetKey(KeyCode.R))
		{
			this.gameObject.transform.position = new Vector3(0, 0, 0);
		}

		//sent velocity to animator
		//animator.SetFloat("Velocity", UpdateMovement());  
	}

	//face character along input direction
	void RotateTowardsMovementDir()
	{
		if(!isPaused)
		{
			if(inputVec != Vector3.zero)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVec), Time.deltaTime * rotationSpeed);
			}
		}
	}
	
	//movement of character
	float UpdateMovement()
	{
		//get movement input from controls
		Vector3 motion = inputVec;  

		//reduce input for diagonal movement
		motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1) ? 0.7f : 1;
		
		if(!isPaused)
		{
			//if not paused, face character along input direction
			RotateTowardsMovementDir();
		}

		return inputVec.magnitude;
	}

	void OnGUI()
	{
		if(charState == CharacterState.Idle && !isMoving)
		{
			isPaused = false;
			if(GUI.Button(new Rect(25, 25, 150, 30), "Pickup Box"))
			{
				animator.SetTrigger("CarryPickupTrigger");
				StartCoroutine(COMovePause(1.2f));
				//StartCoroutine(COShowItem("box", .5f));
				charState = CharacterState.Box;
			}
			if(GUI.Button(new Rect(25, 65, 150, 30), "Recieve Box"))
			{
				animator.SetTrigger("CarryRecieveTrigger");
				StartCoroutine(COMovePause(1.2f));
				//StartCoroutine(COShowItem("box", .5f));
				charState = CharacterState.Box;
			}
		}
		if(charState == CharacterState.Box && !isMoving)
		{
			if(GUI.Button(new Rect(25, 25, 150, 30), "Put Down Box"))
			{
				animator.SetTrigger("CarryPutdownTrigger");
				StartCoroutine(COMovePause(1.2f));
				//StartCoroutine(COShowItem("none", .7f));
				charState = CharacterState.Idle;
			}
			if(GUI.Button(new Rect(25, 65, 150, 30), "Give Box"))
			{
				animator.SetTrigger("CarryHandoffTrigger");
				StartCoroutine(COMovePause(1.2f));
				//StartCoroutine(COShowItem("none", .6f));
				charState = CharacterState.Idle;
			}
		}
	}

	public IEnumerator COMovePause(float pauseTime)
	{
		isPaused = true;
		yield return new WaitForSeconds(pauseTime);
		isPaused = false;
	}

	public IEnumerator COChangeCharacterState(float waitTime, CharacterState state)
	{
		yield return new WaitForSeconds(waitTime);
		charState = state;
	}

	//public IEnumerator COShowItem(string item, float waittime)
	//{
	//	yield return new WaitForSeconds(waittime);
		
	//	if(item == "none")
	//	{
	//		box.SetActive(false);
	//	}
	//	else if(item == "box")
	//	{
	//		box.SetActive(true);
	//	}

	//	yield return null;
	//}
}