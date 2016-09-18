using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour {

    private Color RED;
    private Color ORANGE ;
    private Color YELLOW_ORANGE ;
    private Color YELLOW ;
    private Color GREEN ;
    private Color BLUE ;
    private Color PURPLE ;

    public GameObject controllerPivot;
    public GameObject messageCanvas;
    public Text messageText;
    public GameObject gameMenu;
    public GameObject gameSubMenu;
    public GameObject cursor;
    public GameObject selectedObject;
    private Color currentColor;
    private RaycastHit heldObject;
    private Vector2 touchPos;
    private List<GameObject> buttons = new List<GameObject>();
    private List<GameObject> meshButtons = new List<GameObject>();
    private int currentImage;

    public Transform HackPrefab;
    public Transform HorsePrefab;
    public Transform PigPrefab;
    public Transform TurtlePrefab;
    public Transform CubePrefab;
    public Transform SpherePrefab;

    private bool painting;
    private bool interpolation;
    private bool grabbing;
    private bool hold;
    private float timeElapsed;
    
	void Start () {
        timeElapsed = 0;
        currentImage = 0;
        foreach (Transform child in gameSubMenu.transform) {
            buttons.Add(child.gameObject);
            if (child.tag == "MeshButton") {
                meshButtons.Add(child.gameObject);
            }
        }
        print(buttons.Count);
        print(meshButtons.Count);
        this.currentColor = Color.white;
        RED = new Color(127.0f/255.0f, 20.0f/255.0f, 62 / 255.0f);
        ORANGE = new Color(218.0f/255.0f, 65.0f/255.0f, 82 / 255.0f);
        YELLOW_ORANGE = new Color(236.0f/255.0f, 143.0f/255.0f, 80 / 255.0f);
        YELLOW = new Color(236.0f/255.0f, 205.0f/255.0f, 80 / 255.0f);
        GREEN = new Color(131.0f/255.0f, 165.0f/255.0f, 49 / 255.0f);
        BLUE = new Color(27.0f/255.0f, 166.0f/255.0f, 230 / 255.0f);
        PURPLE = new Color(98.0f/255.0f, 70.0f/255.0f, 162 / 255.0f);

        painting = true;
        interpolation = true;
        grabbing = false;
        hold = false;
    }
	
	// Update is called once per frame
	void Update () {

        UpdateMeshButton();
        UpdatePointer();
        UpdateStatusMessage();
        if (GvrController.IsTouching && painting) {
            ColorHitVertices();
        }
        if (GvrController.IsTouching && grabbing) {
            GrabAndTransform();
        }
    }
    private void GrabAndTransform() {
        RaycastHit hit;
        //Vector3 rayDirection = GvrController.Orientation * Vector3.forward;
        Vector3 rayDirection = -Camera.main.transform.position + cursor.transform.position;
        print(rayDirection);
        if (Physics.Raycast(Camera.main.transform.position, rayDirection, out hit)) {
            
            if (hit.collider && hit.collider.gameObject) {
                print("Grab");
                if (hit.collider.tag == "mesh") {
                    MeshFilter viewedModelFilter = (MeshFilter)hit.collider.gameObject.GetComponent("MeshFilter");
                    Mesh mesh = viewedModelFilter.mesh;
                    hit.transform.parent = cursor.transform;
                    heldObject = hit;
                    hold = true;
                }
            }
        } else {
        }
    }
    private void UpdateMeshButton() {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > 1) {
            timeElapsed = 0;
            currentImage++;
            if (currentImage == 7) {
                currentImage = 0;
            }
        }

        for (int i = 0; i < meshButtons.Count; ++i) {
            if (i == currentImage) {
                meshButtons[i].SetActive(true);
            } else {
                meshButtons[i].SetActive(false);
            }
        }
    }

    private void UpdatePointer() {
        if (GvrController.State != GvrConnectionState.Connected) {
            controllerPivot.SetActive(false);
        }
        controllerPivot.SetActive(true);
        controllerPivot.transform.rotation = GvrController.Orientation;
        

        if (GvrController.ClickButton) {
            painting = false;
            // Show Radial Menu
            // Allow moving of touch direction to activate certain buttons on menu
            touchPos = GvrController.TouchPos;
            gameMenu.SetActive(true);

            Vector2 corrected = touchPos - new Vector2(0.5f, 0.5f);
            float angle = Mathf.DeltaAngle(Mathf.Atan2(1, 0) * Mathf.Rad2Deg,
                               Mathf.Atan2(corrected.y, corrected.x) * Mathf.Rad2Deg) + 180;
            int curr;
            if (corrected.magnitude < .2) {
                curr = 16;
            } else if (angle > 16.365 && angle <= 49.105) {
                curr = 1;
            } else if (angle > 49.105 && angle <= 81.835) {
                curr = 0;
            } else if (angle > 81.835 && angle <= 114.565) {
                curr = 10;
            } else if (angle > 114.565 && angle <= 147.295) {
                curr = 9;
            } else if (angle > 147.295 && angle <= 180.025) {
                curr = 8;
            } else if (angle > 180.025 && angle <= 212.755) {
                curr = 7;
            } else if (angle > 212.755 && angle <= 245.485) {
                curr = 6;
            } else if (angle > 245.485 && angle <= 278.215) {
                curr = 5;
            } else if (angle > 278.215 && angle <= 310.945) {
                curr = 4;
            } else if (angle > 310.945 && angle <= 343.675) {
                curr = 3;
            } else {
                curr = 2;
            }
            bool isMeshButton = buttons[curr].tag == "MeshButton";
            for (int i = 0; i < buttons.Count; ++i) {
                if (i == curr) {
                    if (buttons[i].tag == "MeshButton") {
                        foreach (GameObject go in meshButtons) {
                            go.transform.localScale = new Vector3(1.2f, 1.2f);
                        }
                    }
                    buttons[i].transform.localScale = new Vector3(1.2f, 1.2f);
                    if (curr == 16) {
                        buttons[i].transform.localScale = new Vector3(1.4f, 1.4f);
                    }
                } else {
                    if (isMeshButton) {
                        if (buttons[i].tag == "MeshButton") {
                            continue;
                        }
                    }
                    buttons[i].transform.localScale = new Vector3(1, 1);
                }
            }
        } else {
            painting = true;
            gameMenu.SetActive(false);
        }
        if (GvrController.TouchUp) {
            if (hold) {
                heldObject.transform.parent = null;
                hold = false;
            }
        }
        if (GvrController.AppButton) {
            Camera.main.transform.position += new Vector3(Camera.main.transform.forward.x*.05f, 0, Camera.main.transform.forward.z *.05f);
            controllerPivot.transform.position += new Vector3(Camera.main.transform.forward.x * .05f, 0, Camera.main.transform.forward.z * .05f);
        }

        if (GvrController.ClickButtonUp) {
            // Change color to the last touch position
            Vector2 corrected = touchPos - new Vector2(0.5f, 0.5f);
            float angle = Mathf.DeltaAngle(Mathf.Atan2(1, 0) * Mathf.Rad2Deg,
                               Mathf.Atan2(corrected.y, corrected.x) * Mathf.Rad2Deg) + 180;
            if (corrected.magnitude < 0.2) {
                StartCoroutine("CaptureScreenshot");
            } else if (angle > 16.365 && angle <= 49.105) {
                currentColor = ORANGE;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 49.105 && angle <= 81.835) {
                currentColor = YELLOW_ORANGE;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 81.835 && angle <= 114.565) {
                //grab
                grabbing = true;
                interpolation = true;
                painting = false;
                
            } else if (angle > 114.565 && angle <= 147.295) {
                //eraser
                grabbing = false;
                interpolation = false; // index 9
                currentColor = Color.white;
            } else if (angle > 147.295 && angle <= 180.025) {
                //mesh
                grabbing = false;
                painting = false;
                GameObject clone;
                switch (currentImage) {
                    
                case 0:
                      clone = Instantiate(HackPrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.48f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.Euler(0, 180, 0)) as GameObject;
                        //clone.tag = "mesh";
                    break;
                case 1:
                        clone = Instantiate(HorsePrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.89f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.identity) as GameObject;
                        //clone.tag = "mesh";
                        break;
                case 2:
                        clone = Instantiate(PigPrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.25f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.Euler(0, 90, 0)) as GameObject;
                        //clone.tag = "mesh";
                        break;
                case 3:
                        clone = Instantiate(TurtlePrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.27f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.identity) as GameObject;
                        //clone.tag = "mesh";
                        break;
                case 4:
                        clone = Instantiate(CubePrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.2f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.identity) as GameObject;
                        //clone.tag = "mesh";
                        break;
                case 5:
                        clone = Instantiate(SpherePrefab, new Vector3(
                        cursor.transform.position.x + Camera.main.transform.forward.x, 0.22f,
                        cursor.transform.position.z + Camera.main.transform.forward.z),
                        Quaternion.identity) as GameObject;
                        //clone.tag = "mesh";
                        break;
                default:
                    break;
                }

            } else if (angle > 180.025 && angle <= 212.755) {
                currentColor = YELLOW;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 212.755 && angle <= 245.485) {
                currentColor = GREEN;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 245.485 && angle <= 278.215) {
                currentColor = BLUE;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 278.215 && angle <= 310.945) {
                currentColor = PURPLE;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else if (angle > 310.945 && angle <= 343.675) {
                currentColor = Color.black;
                interpolation = true;
                painting = true;
                grabbing = false;
            } else {
                currentColor = RED;
                interpolation = true;
                painting = true;
                grabbing = false;
            }

            
        }
        cursor.GetComponent<Renderer>().material.color = currentColor;
    }

    private void ColorHitVertices() {
        RaycastHit hit;
        //Vector3 rayDirection = GvrController.Orientation * Vector3.forward;
        Vector3 rayDirection = -Camera.main.transform.position + cursor.transform.position;
        print(rayDirection);
        if (Physics.Raycast(Camera.main.transform.position, rayDirection, out hit)) {
            if (hit.collider && hit.collider.gameObject) {
                print("Hit object");
                MeshFilter viewedModelFilter = (MeshFilter)hit.collider.gameObject.GetComponent("MeshFilter");
                Mesh mesh = viewedModelFilter.mesh;
                if (mesh.colors == null) {
                    print("no color");
                    return;
                } else {
                    print("size " + mesh.colors.Length);
                    if (mesh.colors.Length == 0) {
                        Color[] colorArr = new Color[mesh.vertexCount];
                        for (int i = 0; i < mesh.vertexCount; i++) {
                            colorArr[i] = Color.white;
                        }
                        mesh.colors = colorArr;
                    }
                }
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                print("Triangles " + triangles.Length);
                print("Index " + triangles[hit.triangleIndex * 3]);
                Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
                Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
                Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

                Transform hitTransform = hit.collider.transform;
                p0 = hitTransform.TransformPoint(p0);
                p1 = hitTransform.TransformPoint(p1);
                p2 = hitTransform.TransformPoint(p2);
                Color[] c = new Color[mesh.vertexCount];
                for (int i = 0; i < mesh.vertexCount; i++) {
                    c[i] = mesh.colors[i];
                }
                if (interpolation) {
                    c[triangles[hit.triangleIndex * 3 + 0]] = Color.Lerp(c[triangles[hit.triangleIndex * 3 + 0]], currentColor, 0.3f);
                    c[triangles[hit.triangleIndex * 3 + 1]] = Color.Lerp(c[triangles[hit.triangleIndex * 3 + 0]], currentColor, 0.3f);
                    c[triangles[hit.triangleIndex * 3 + 2]] = Color.Lerp(c[triangles[hit.triangleIndex * 3 + 0]], currentColor, 0.3f);
                }
                else {
                    c[triangles[hit.triangleIndex * 3 + 0]] = currentColor;
                    c[triangles[hit.triangleIndex * 3 + 1]] = currentColor;
                    c[triangles[hit.triangleIndex * 3 + 2]] = currentColor;
                }
                
                mesh.colors = c;

            }
        } else {
        }
    }

    private IEnumerator CaptureScreenshot() {
        for (int i = 0; i < 2; ++i) {
            yield return new WaitForSeconds(.2f);
        }
        Application.CaptureScreenshot("Screenshot.png");
        print("screenshot captured");
    }

    private void UpdateStatusMessage() {
        // This is an example of how to process the controller's state to display a status message.
        switch (GvrController.State) {
        case GvrConnectionState.Connected:
            messageCanvas.SetActive(false);
            break;
        case GvrConnectionState.Disconnected:
            messageText.text = "Controller disconnected.";
            messageText.color = Color.white;
            messageCanvas.SetActive(true);
            break;
        case GvrConnectionState.Scanning:
            messageText.text = "Controller scanning...";
            messageText.color = Color.cyan;
            messageCanvas.SetActive(true);
            break;
        case GvrConnectionState.Connecting:
            messageText.text = "Controller connecting...";
            messageText.color = Color.yellow;
            messageCanvas.SetActive(true);
            break;
        case GvrConnectionState.Error:
            messageText.text = "ERROR: " + GvrController.ErrorDetails;
            messageText.color = Color.red;
            messageCanvas.SetActive(true);
            break;
        default:
            // Shouldn't happen.
            Debug.LogError("Invalid controller state: " + GvrController.State);
            break;
        }
    }

}
