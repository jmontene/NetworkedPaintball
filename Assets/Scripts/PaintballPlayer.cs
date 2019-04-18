using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PaintballPlayer : NetworkBehaviour
{
    public float speed = 5.0f;
    public float rotationSpeed = 1f;
    public int ammo = 15;
    public Transform cameraPos;
    public GameObject projectilePrefab;

    public Renderer hurtRenderer;
    public float hurtTime;

    private Rigidbody m_rb = null;
    private Renderer m_renderer = null;
    private GameObject ammoParent = null;
    private Text ammoText = null;
    private int maxAmmo;

    [SyncVar]
    [HideInInspector]
    public Color playerColor;

    [HideInInspector]
    public bool gameStarted;

    private void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_renderer = GetComponent<Renderer>();
        gameStarted = false;
        ChangeToColor(playerColor);
    }

    private void Update()
    {
        if (!hasAuthority)
        {
            return;
        }

        if (!gameStarted) return;

        Vector3 forwardVelocity = transform.forward * Input.GetAxis("Vertical") * speed;
        Vector3 yRot = new Vector3(0f, rotationSpeed * Input.GetAxis("Horizontal"));

        if (Input.GetMouseButtonDown(0) && ammo > 0)
        {
            ammo--;
            ammoText.text = "Ammo X " + ammo;
            CmdShoot(m_renderer.material.color);
        }

        m_rb.rotation = Quaternion.Euler(m_rb.rotation.eulerAngles + yRot);
        m_rb.velocity = forwardVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            PaintBallProjectile proj = other.GetComponent<PaintBallProjectile>();
            if(proj.alliedColor != m_renderer.material.color)
            {
                StopAllCoroutines();
                StartCoroutine(HurtCo(proj.alliedColor));
            }
        }else if (other.CompareTag("Pickup"))
        {
            if (hasAuthority)
            {
                ammo = maxAmmo;
                ammoText.text = "Ammo X " + ammo;
                NetworkServer.Destroy(other.gameObject);
            }
        }
    }

    IEnumerator HurtCo(Color c)
    {
        if (hasAuthority)
        {
            CmdUpdateScore(c);
        }

        hurtRenderer.material.color = c;
        float time = 0f;
        while(time < hurtTime)
        {
            time += Time.deltaTime;
            hurtRenderer.material.color = Color.Lerp(c, Color.white, time / hurtTime);
            yield return null;
        }
    }

    [Command]
    public void CmdUpdateScore(Color c)
    {
        PaintBallNetworkManager man = (PaintBallNetworkManager)(NetworkManager.singleton);
        man.AddScore(c);
    }

    public void OnGameStarted()
    {
        gameStarted = true;
        AlignCameraToPlayer();

        maxAmmo = ammo;
        ammoParent = GameObject.Find("AmmoPanel");
        ammoParent.GetComponent<Image>().enabled = true;
        ammoText = GameObject.Find("AmmoText").GetComponent<Text>();
        ammoText.enabled = true;
        ammoText.text = "Ammo X " + ammo;
    }

    public void OnGameEnded()
    {
        gameStarted = false;
        ammoParent.SetActive(false);
    }

    void ChangeToColor(Color c)
    {
        GetComponent<Renderer>().material.color = c;
        playerColor = c;
        if(hasAuthority) CmdChangeColor(c);
    }

    public void AlignCameraToPlayer()
    {
        Camera cam = Camera.main;
        cam.transform.parent = transform;
        cam.transform.localPosition = cameraPos.localPosition;
        cam.transform.localRotation = cameraPos.localRotation;
    }

    [Command]
    public void CmdShoot(Color c)
    {
        GameObject obj = Instantiate<GameObject>(projectilePrefab, transform.position, transform.rotation);
        PaintBallProjectile proj = obj.GetComponent<PaintBallProjectile>();

        NetworkServer.Spawn(obj);
        proj.SetColor(c);
    }

    [Command]
    void CmdChangeColor(Color c)
    {
        m_renderer.material.color = c;
        RpcChangeColor(c);
    }

    [ClientRpc]
    void RpcChangeColor(Color c)
    {
        m_renderer.material.color = c;
    }
}
