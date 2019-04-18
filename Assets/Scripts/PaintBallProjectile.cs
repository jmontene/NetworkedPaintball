using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PaintBallProjectile : NetworkBehaviour
{
    public float speed = 10f;
    public float timeToLive = 3f;

    [SyncVar]
    public Color alliedColor;

    Rigidbody m_rb;

    // Start is called before the first frame update
    void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        m_rb.velocity = transform.forward * speed;
        timeToLive -= Time.deltaTime;
        if(timeToLive <= 0f)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public void SetColor(Color c)
    {
        RpcSetColor(c);
    }

    [ClientRpc]
    public void RpcSetColor(Color c)
    {
        alliedColor = c;
        GetComponent<Renderer>().material.color = alliedColor;
    }
}
