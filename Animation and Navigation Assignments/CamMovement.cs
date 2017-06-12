using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour {

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    public Camera camera;
    public Material d;
    public Material selected;

    private List<GameObject> list = new List<GameObject>();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(1))
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            if (Input.GetKey(KeyCode.W))
            {
                gameObject.transform.Translate(Vector3.forward * Time.deltaTime * 20);
            }
            if (Input.GetKey(KeyCode.S))
            {
                gameObject.transform.Translate(Vector3.back * Time.deltaTime * 20);
            }
            if (Input.GetKey(KeyCode.D))
            {
                gameObject.transform.Translate(Vector3.right * Time.deltaTime * 20);
            }
            if (Input.GetKey(KeyCode.A))
            {
                gameObject.transform.Translate(Vector3.left * Time.deltaTime * 20);
            }
        }
    }

    public float distance = 50f;
    //replace Update method in your class with this one
    void FixedUpdate()
    {
        //if mouse button (left hand side) pressed instantiate a raycast
        if (Input.GetMouseButtonDown(0))
        {
            //create a ray cast and set it to the mouses cursor position in game
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                //draw invisible ray cast/vector
                Debug.DrawLine(ray.origin, hit.point);
                //log hit area to the console
                Debug.Log(hit.collider.gameObject.name);
                if (hit.collider.gameObject.tag.Equals("AI"))
                {
                    list.Add(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<Renderer>().material = selected;
                }else
                {
                    if (list.Count>0)
                    {
                        foreach(GameObject go in list)
                        {
                            go.GetComponent<Agent>().go(hit.point);
                            go.GetComponent<Renderer>().material = d;
                        }
                        list.Clear();
                    }
                }

            }
        }
    }
}
