using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Animations.Rigging;
using Photon.Pun;

public class ThirdPersonController : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Controls")]
    public float normalSensitivity = 15.0f;
    public float aimSensitivity = 7.0f;
    public float aimRange = 99.0f;

    [Header("Gameplay")]
    public float speed = 3.0f;
    public float runSpeed = 6.0f;
    public int maxAmmo = 24;
    public float maxHealth = 5.0f;
    public float fireRate = 2.0f;
    public float bulletDamage = .1f;
    public float bulletSpeed = 35.0f;
    public float shootMinPitch = .75f;
    public float shootMaxPitch = 1.15f;

    public GameObject bulletPrefab;

    [Header("Sound")]
    public AudioClip shootClip;
    public AudioClip noAmmoClip;
    public AudioClip reloadClip;

    public AudioClip[] footsteps;

    [Header("Local Components")]
    public Transform followCameraTarget;
    public Transform aimTarget;
    public Transform bulletOrigin;
    public MultiAimConstraint handConstraint;
    public MultiAimConstraint bodyConstraint;
    public MultiAimConstraint headConstraint;
    public CinemachineImpulseSource cannonImpulse;
    public AudioSource cannonAudioSource;
    public AudioSource footstepsAudioSource;
    public Light weaponPointLight;
    public ParticleSystem weaponSmoke;
    public ParticleSystem ammoShell;

    [HideInInspector]
    public CinemachineVirtualCamera normalVirtualCamera;
    [HideInInspector]
    public CinemachineVirtualCamera aimVirtualCamera;

    [HideInInspector]
    public PlayerInputActions playerInput;
    private Rigidbody body;
    private Animator animator;

    private RaycastHit hit;
    private bool rightShoulder = true;
    private bool aiming = false;
    private bool firing = false;
    private bool reloading = false;
    private bool running = false;
    private Vector2 movement = Vector2.zero;
    [HideInInspector]
    public int currentAmmo = 0;
    [HideInInspector]
    public float currentHealth = 0.0f;

    public delegate void HitEvent(float currentHealt, float maxHealth);
    public event HitEvent OnHit;

    public delegate void ShootEvent(int currentAmmo, int maxAmmo);
    public event ShootEvent OnShoot;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        animator.SetFloat("FireRate", fireRate);

        currentAmmo = maxAmmo;
        currentHealth = maxHealth;
        weaponPointLight.enabled = false;

        playerInput = new PlayerInputActions();

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        playerInput.Player.Aim.started += Aim;
        playerInput.Player.Aim.canceled += Aim;
        playerInput.Player.Fire.performed += Fire;
        playerInput.Player.ChangeShoulder.performed += ChangeShoulder;
        playerInput.Player.Sprint.performed += ToggleRun;
        playerInput.Player.Reload.performed += Reload;

        playerInput.UI.Scoreboard.started += ShowScores;
        playerInput.UI.Scoreboard.canceled += ShowScores;
        playerInput.UI.Pause.performed += ShowPause;

        playerInput.Enable();
    }

    private void Update()
    {
        if (currentHealth <= 0.0f)
        {
            return;
        }

        animator.SetBool("Aiming", aiming);
        animator.SetBool("Firing", firing);
        animator.SetBool("Reloading", reloading);
        animator.SetBool("Running", running);

        if (aiming)
        {
            handConstraint.weight = 1.0f;
            headConstraint.weight = 1.0f;
            bodyConstraint.weight = 0.873f;
        }
        else
        {
            handConstraint.weight = 0.0f;
            headConstraint.weight = 0.0f;
            bodyConstraint.weight = 0.0f;
        }

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        // camera rotation
        Vector2 rotation = playerInput.Player.Rotate.ReadValue<Vector2>() * (aiming ? aimSensitivity : normalSensitivity) * Time.deltaTime;

        followCameraTarget.transform.rotation *= Quaternion.AngleAxis(rotation.x, Vector3.up);
        followCameraTarget.transform.rotation *= Quaternion.AngleAxis(rotation.y, Vector3.right);

        Vector3 angles = followCameraTarget.transform.localEulerAngles;
        angles.z = 0.0f;

        if (angles.x > 180.0f && angles.x < 340.0f)
        {
            angles.x = 340.0f;
        }
        else if (angles.x < 180.0f && angles.x > 40.0f)
        {
            angles.x = 40.0f;
        }

        followCameraTarget.localEulerAngles = angles;

        // movement
        movement = playerInput.Player.Movement.ReadValue<Vector2>();

        if (aiming || movement != Vector2.zero)
        {
            followCameraTarget.transform.parent = null;

            if (aiming)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0.0f, followCameraTarget.eulerAngles.y)), Time.deltaTime * 20.0f);
                
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
                if (Physics.Raycast(ray, out hit, aimRange))
                {
                    aimTarget.position = hit.point;
                }
                else
                {
                    aimTarget.position = ray.origin + ray.direction * aimRange;
                }
            }
            else
            {
                transform.rotation = Quaternion.Euler(new Vector3(0.0f, followCameraTarget.eulerAngles.y));
            }

            followCameraTarget.transform.parent = transform;
        }

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
    }

    private void FixedUpdate()
    {
        body.velocity = Vector3.zero;
        body.angularDrag = 0.0f;
        body.angularVelocity = Vector3.zero;

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        //if (!aiming)
        //{
            movement *= (running ? runSpeed : speed) * Time.fixedDeltaTime;
            body.MovePosition(transform.position + (transform.forward * movement.y) + (transform.right * movement.x));
        //}
    }

    private void OnDestroy()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        playerInput.Player.Aim.started -= Aim;
        playerInput.Player.Aim.canceled -= Aim;
        playerInput.Player.Fire.performed -= Fire;
        playerInput.Player.ChangeShoulder.performed -= ChangeShoulder;
        playerInput.Player.Sprint.performed -= ToggleRun;
        playerInput.Player.Reload.performed -= Reload;

        playerInput.UI.Scoreboard.started -= ShowScores;
        playerInput.UI.Scoreboard.canceled -= ShowScores;
        playerInput.UI.Pause.performed -= ShowPause;
    }

    #region PlayerInput

    private void Aim(InputAction.CallbackContext context)
    {
        // We ignore canceled actions in case that we have toggled the aim (reload) and the player has been holding the button, the other option is to togle back the aim after reloading if they still have the button pressed
        if (context.canceled && !aiming)
        {
            return;
        }

        if (reloading)
        {
            return;
        }

        ToggleAim();
    }

    private void Fire(InputAction.CallbackContext obj)
    {
        if (!aiming)
        {
            return;
        }

        firing = !firing;
        animator.SetBool("Firing", firing);
    }

    private void ChangeShoulder(InputAction.CallbackContext obj)
    {
        rightShoulder = !rightShoulder;
        StopCoroutine(ChangeSide());
        StartCoroutine(ChangeSide());
    }

    private void ToggleRun(InputAction.CallbackContext obj)
    {
        running = !running;
    }

    private void Reload(InputAction.CallbackContext obj)
    {
        if (reloading)
        {
            return;
        }

        if (aiming || firing)
        {
            ToggleAim();
        }

        reloading = true;
        animator.SetBool("Reloading", reloading);
    }

    private void ShowScores(InputAction.CallbackContext obj)
    {
        GameManager.instance.ShowScores();
    }

    private void ShowPause(InputAction.CallbackContext obj)
    {
        GameManager.instance.ShowPause();
    }

    #endregion

    #region PrivateMethods

    private void ToggleAim()
    {
        aiming = !aiming;

        aimVirtualCamera.gameObject.SetActive(aiming);

        if (aiming)
        {
            //playerInput.Player.Movement.Disable();
            animator.SetBool("Aiming", aiming);
            handConstraint.weight = 1.0f;
            headConstraint.weight = 1.0f;
            bodyConstraint.weight = 0.873f;
        }
        else
        {
            //playerInput.Player.Movement.Enable();
            animator.SetBool("Aiming", aiming);
            handConstraint.weight = 0.0f;
            headConstraint.weight = 0.0f;
            bodyConstraint.weight = 0.0f;

            firing = false;
            animator.SetBool("Firing", firing);
        }
    }

    public void Default()
    {
        rightShoulder = true;
        aiming = false;
        firing = false;
        reloading = false;
        running = false;
        movement = Vector2.zero;
        currentAmmo = maxAmmo;
        currentHealth = maxHealth;

        aimVirtualCamera.gameObject.SetActive(false);        
        weaponPointLight.enabled = false;
    }

    #endregion

    #region AnimationEvents

    private void Shoot()
    {
        if (currentAmmo <= 0)
        {
            cannonAudioSource.clip = noAmmoClip;
            cannonAudioSource.pitch = Random.Range(.75f, 1.15f);
        }
        else
        {
            cannonAudioSource.pitch = Random.Range(shootMinPitch, shootMaxPitch);
        }

        cannonAudioSource.Play();

        if (currentAmmo <= 0)
        {
            return;
        }

        cannonImpulse.GenerateImpulse();
        weaponPointLight.enabled = true;

        weaponSmoke.Play();
        ammoShell.Stop();
        ammoShell.Play();

        StartCoroutine(TurnOffLight());

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        GameObject bullet; 
        if (PhotonNetwork.IsConnected)
        {
            bullet = PhotonNetwork.Instantiate(bulletPrefab.name, bulletOrigin.position, Quaternion.identity);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, bulletOrigin.position, Quaternion.identity);

        }

        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.damage = bulletDamage;
        bulletController.speed = bulletSpeed;

        bullet.transform.LookAt(aimTarget);
        currentAmmo -= 1;

        OnShoot?.Invoke(currentAmmo, maxAmmo);
    }

    public void StartReload()
    {
        cannonAudioSource.clip = reloadClip;
        cannonAudioSource.Play();
    }

    public void EndReload()
    {
        cannonAudioSource.clip = shootClip;
        reloading = false;
        animator.SetBool("Reloading", reloading);

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        currentAmmo = maxAmmo;
        OnShoot?.Invoke(currentAmmo, maxAmmo);
    }

    public void PlayFootSteep()
    {
        footstepsAudioSource.pitch = Random.Range(0.75f, 1.15f);
        footstepsAudioSource.clip = footsteps[Random.Range(0, footsteps.Length)];
        footstepsAudioSource.Play();
    }

    #endregion

    #region Networking

    [PunRPC]
    public void TakeDamage(float damage, string killerNickname)
    {
        currentHealth -= damage;

        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        if (currentHealth <= 0.0f)
        {
            GameManager.instance.ShowKill(killerNickname, photonView.Owner.NickName);

            if (killerNickname == PhotonNetwork.NickName)
            {
                ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
                int kills = (int)properties["kills"];
                properties["kills"] = kills + 1;

                PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            }
        }

        if (!photonView.IsMine)
        {
            return;
        }

        if (currentHealth <= 0.0f)
        {
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
            int deaths = (int) properties["deaths"];
            properties["deaths"] = deaths + 1;
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

            GameManager.instance.Respawn();

            //GameManager.instance.LeaveRoom();
        }

        OnHit?.Invoke(currentHealth, maxHealth);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentHealth);
            stream.SendNext(currentAmmo);
            stream.SendNext(firing);
            stream.SendNext(aiming);
            stream.SendNext(reloading);
            stream.SendNext(running);
        }
        else
        {
            currentHealth = (float)stream.ReceiveNext();
            currentAmmo = (int)stream.ReceiveNext();
            firing = (bool)stream.ReceiveNext();
            aiming = (bool)stream.ReceiveNext();
            reloading = (bool)stream.ReceiveNext();
            running = (bool)stream.ReceiveNext();
        }
    }

    #endregion

    #region Corutines

    private IEnumerator ChangeSide()
    {
        Cinemachine3rdPersonFollow aimFollowCameraComponent = aimVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        Cinemachine3rdPersonFollow normalFollowCameraComponent = normalVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        float side = normalFollowCameraComponent.CameraSide;
        float desiredSide = (rightShoulder ? 1.0f : 0.0f);
        float t = 0.0f;
        while (side != desiredSide)
        {
            side = Mathf.Lerp(side, desiredSide, t);

            normalFollowCameraComponent.CameraSide = side;
            aimFollowCameraComponent.CameraSide = side;

            t += 0.01f;
            yield return null;
        }
    }

    private IEnumerator TurnOffLight()
    {
        yield return new WaitForSeconds(.1f);
        weaponPointLight.enabled = false;
    }

    #endregion
}
