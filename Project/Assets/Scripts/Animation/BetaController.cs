using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BetaController : NetworkBehaviour {

	private Animator anim;

	private Camera camCamera;

	public Camera cam;

	public bool IKActive;
	public Transform sight;
	public Transform leftKnee;
	public Transform rightKnee;
	public float IKWeight;

	public float offsetY;

	Vector3 leftPosition;
	Vector3 rightPosition;

	Quaternion leftRotation;
	Quaternion rightRotation;

	float leftWeight;
	float rightWeight;

	Transform leftFoot;
	Transform rightFoot;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> (); 
		camCamera = cam.GetComponent<Camera> ();
		leftFoot = anim.GetBoneTransform (HumanBodyBones.LeftFoot);
		rightFoot = anim.GetBoneTransform (HumanBodyBones.RightFoot);

	}

	void Update() {

		if (!isLocalPlayer) {
			camCamera.enabled = false;
			return;
		}
		InfiniteTerrain.viewer = transform;
	}

	void FixedUpdate () {
		
		if (!isLocalPlayer)
		{
			return;
		}

		float vertical = Input.GetAxis ("Vertical");
		float horizontal = Input.GetAxis ("Horizontal");
		bool leftShift = Input.GetKey (KeyCode.LeftShift);
		anim.SetFloat ("VerticalSpeed", vertical);
		anim.SetFloat ("HorizontalSpeed", horizontal);
		anim.SetBool ("ShiftKeyDown", leftShift);
		anim.SetBool ("SpaceKeyDown", Input.GetKey (KeyCode.Space));
		anim.SetBool ("IsIdle", vertical == 0);
		anim.SetBool ("IsStrafing", vertical != 0 && horizontal != 0);
		anim.SetBool ("IsTurning", vertical == 0 && horizontal != 0);

		/*int mult = leftShift && vertical != 0 ? 5 : 1;
		float translation = vertical * speed * mult;
		float rotation = horizontal * rotationSpeed * mult;
		translation *= Time.deltaTime;
		rotation *= Time.deltaTime;
		if (horizontal == 0 && vertical > 0 || vertical < 0) {
			transform.Translate (0, 0, translation);
		} else if (vertical > 0) {
			transform.Translate (rotation / 100f, 0, translation);
		}
		if (vertical == 0 && horizontal != 0) {
			transform.Rotate (0, rotation, 0);
		}*/

		RaycastHit leftHit;
		RaycastHit rightHit;

		Vector3 leftPos = leftFoot.TransformPoint (Vector3.zero);
		Vector3 rightPos = rightFoot.TransformPoint (Vector3.zero);

		if (Physics.Raycast (leftPos, Vector3.down, out leftHit, 1)) {
			leftPosition = leftHit.point;
			leftRotation = Quaternion.FromToRotation (transform.up, leftHit.normal) * transform.rotation;
		}
		if(Physics.Raycast (rightPos, Vector3.down, out rightHit, 1)) {
			rightPosition = rightHit.point;
			rightRotation = Quaternion.FromToRotation (transform.up, rightHit.normal) * transform.rotation;
		}
	}

	void OnAnimatorIK()
	{
		if(anim) {
			if (IKActive) {
				if (sight != null) {
					anim.SetLookAtWeight (IKWeight);
					anim.SetLookAtPosition (sight.position);
				}

				leftWeight = anim.GetFloat ("leftFoot");
				rightWeight = anim.GetFloat ("rightFoot");

				anim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, (1-leftWeight)*2);
				anim.SetIKPositionWeight (AvatarIKGoal.RightFoot, (1-rightWeight)*2);

				anim.SetIKPosition (AvatarIKGoal.LeftFoot, leftPosition + new Vector3(0, offsetY));
				anim.SetIKPosition (AvatarIKGoal.RightFoot, rightPosition + new Vector3(0, offsetY));

				anim.SetIKRotationWeight (AvatarIKGoal.LeftFoot, (1-leftWeight)*2);
				anim.SetIKRotationWeight (AvatarIKGoal.RightFoot, (1-rightWeight)*2);

				anim.SetIKRotation (AvatarIKGoal.LeftFoot, leftRotation);
				anim.SetIKRotation (AvatarIKGoal.RightFoot, rightRotation);

				anim.SetIKHintPositionWeight (AvatarIKHint.LeftKnee, (1-leftWeight));
				anim.SetIKHintPositionWeight (AvatarIKHint.RightKnee, (1-rightWeight));

				anim.SetIKHintPosition (AvatarIKHint.LeftKnee, leftKnee.position);
				anim.SetIKHintPosition (AvatarIKHint.RightKnee, rightKnee.position);

			} else {          
				anim.SetLookAtWeight(0);
			}
		}
	}
}