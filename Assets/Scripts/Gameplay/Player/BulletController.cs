using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class BulletController : MonoBehaviourPunCallbacks
{
    [HideInInspector]
    public float damage = 0.1f;

    [HideInInspector]
    public float speed = 15.0f;

    [SerializeField]
    private float lifetime = 20.0f;

    public LayerMask collisionMask;

    public GameObject blood;
    public GameObject rocks;

    private float spawnTime = 0.0f;

    private CinemachineImpulseSource impulseSource;
    private AudioSource audioSource;
    private Collider boxCollider;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;
    private Vector3 halfExtents;

    private void Awake()
    {
        impulseSource = GetComponentInChildren<CinemachineImpulseSource>();
        audioSource = GetComponent<AudioSource>();

        boxCollider = GetComponent<Collider>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();

        BoxCollider collider = GetComponent<BoxCollider>();
        halfExtents = Vector3.Scale(collider.size, transform.localScale) * .5f;
        collider.enabled = false;
    }

    private void Start()
    {
        spawnTime = Time.time;        
    }

    // Update is called once per frame
    private void Update()
    {
        if (!(spawnTime + lifetime <= Time.time))
        {
            return;
        }

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else if (!PhotonNetwork.IsConnected)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        RaycastHit info;
        bool collision = Physics.BoxCast(transform.position, halfExtents, 
            transform.forward, out info, Quaternion.identity, 
            speed * Time.fixedDeltaTime, collisionMask);

        if (collision)
        {
            //transform.position = info.point /*- transform.forward * halfExtents.z*/;
            Impact(info.transform.gameObject);
        }
        else
        {
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }
    }

    private void Impact(GameObject other = null)
    {
        impulseSource.GenerateImpulse();
        audioSource.pitch = Random.Range(0.75f, 1.15f);
        audioSource.Play();
        speed = 0.0f;

        meshRenderer.enabled = false;
        trailRenderer.enabled = false;
        boxCollider.enabled = false;

        GameObject particles;
        ThirdPersonController controller = other.GetComponent<ThirdPersonController>();
        if (controller)
        {
            particles = Instantiate(blood, transform.position, transform.rotation);
        }
        else
        {
            particles = Instantiate(rocks, transform.position, transform.rotation);
        }

        //particles.transform.forward *= -1.0f;

        if (PhotonNetwork.IsConnected && photonView.IsMine)
        {
            if (controller)
            {
                controller.photonView.RPC("TakeDamage", RpcTarget.All, damage, PhotonNetwork.NickName);
            }

            StartCoroutine(DestroyDelayed());
        }
        else if (!PhotonNetwork.IsConnected)
        {
            if (controller)
            {
                controller.TakeDamage(damage, "");
            }

            Destroy(gameObject, audioSource.clip.length);
        }
    }

    private IEnumerator DestroyDelayed()
    {
        yield return new WaitForSeconds(audioSource.clip.length);
        PhotonNetwork.Destroy(gameObject);
    }
}
