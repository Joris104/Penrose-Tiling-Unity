using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridController : MonoBehaviour
{

    [SerializeField]
    public bool debug = false;
    [SerializeField]
    private int score = 0, mouseScore = 0;
    [SerializeField]
    public int remainingTiles = 120;
    [SerializeField]
    float searchDistance = 1.25f, camHeight, camWidth, targetCamSize;
    [SerializeField]
    Tile.tileTypes currType = Tile.tileTypes.Pentagon;
    [SerializeField]
    GameObject parent, pentagon, star, boat, rhombus;
    [SerializeField]
    GameObject visualizer, scoreText, mouseScoreText, screenShotButton, restartButton;

    public static List<Tile> tiles;
    delegate Tile Add(Vector3 position, float rotation, Color color, bool mouseTile=false);
    Add addTile;
    Add[] adds;

    float north = 0.5f, south = -0.5f, east = -1, west = 1;
    Vector3 camPosTarget = new Vector3(0,0,-10);
    float camMoveSpeed = 1f;

    Vector3 mousePosition;
    GameObject mouseObject;
    public bool activeGame = true;
    Tile mouseTile;
    Color mouseColor;
    
    bool isGlued;
    public bool endgame = false;

   

    GameObject[] visu;
    // Start is called before the first frame update

    List<Tile> getNeighbours(Tile t){
        bool breakFlag = false;
        List<Tile> neighbours = new List<Tile>();
        Vector2[] edges = t.getEdges();
        foreach(Tile n in tiles){
            if(n == t){
                continue;
            }
            breakFlag = false;
            Vector2[] nEdges = n.getEdges();
            foreach(Vector2 e in edges){
                foreach(Vector2 nE in nEdges){
                    if((e - nE).magnitude < 0.1f){
                        neighbours.Add(n);
                        breakFlag = true;
                        break;
                    }
                }
                if(breakFlag){
                   break;
                }
            }
        }

        return neighbours;
    }

    void setMouseTileType(Tile.tileTypes type){
        addTile = adds[(int)type];
        Destroy(mouseObject);
        mouseTile = addTile(mousePosition, 0.0f, mouseColor, true);
        mouseObject = mouseTile.go;
        mouseObject.AddComponent<CollisionDetection>();
        Rigidbody2D rb = mouseObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    void setRandomMouseTileType(){
        if(remainingTiles == 0){
            mouseObject.SetActive(false);
            screenShotButton.SetActive(true);
            restartButton.SetActive(true);
            endgame = true;
            return;
        }
        remainingTiles--;
        mouseScoreText.GetComponent<TextMeshProUGUI>().text = $"{remainingTiles}";
        if(tiles.Count < 5 || currType != Tile.tileTypes.Pentagon){
            currType = (Tile.tileTypes.Pentagon);
            setMouseTileType(currType);
            return;
        }

        int r = Random.Range(0,100);
        if(r < 50){
            currType = (Tile.tileTypes.Pentagon);
        }
        else if(r < 85){
            currType = (Tile.tileTypes.Rhombus);
        }
        else if(r < 95){
            currType = (Tile.tileTypes.Boat);
        }
        
        else {
            currType = (Tile.tileTypes.Star);
        }
        
        
        setMouseTileType(currType);
    }

    void Start()
    {
        tiles = new List<Tile>();
        visu = new GameObject[10];
        mouseColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        adds = new Add[] {addPentagon, addStar, addBoat, addRhombus};
        setMouseTileType(currType);
        addPentagon(Vector3.zero,0f,Color.red);
        /*for(int i = 0; i < visu.Length; i++){
            visu[i] = Instantiate(visualizer,new Vector3(0,0,5),Quaternion.identity);
            visu[i].name = $"{i}";
        }*/
        
        Camera cam = Camera.main;
        targetCamSize = cam.orthographicSize;
        

        scoreText.GetComponent<TextMeshProUGUI>().text = $"0";

    }

    
    bool outsideCamera(Tile t){
        Vector2 p = t.getCenter() - (Vector2) Camera.main.transform.position;
        if(Mathf.Abs(p.x) > camWidth/2 || Mathf.Abs(p.y) > camHeight/2){
                return true;
        }
        /*Vector2[] points = t.getPoints();
        foreach(Vector2 p in points){
            if(Mathf.Abs(p.x) > camWidth/2 || Mathf.Abs(p.y) > camHeight/2){
                return true;
            }

        }*/
        return false;
    }

    void Update()
    {
        if(endgame || !activeGame) return;
        camHeight = 2f * Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
        mouseScore = 0;
        mousePosition=Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseObject.transform.Rotate(Input.mouseScrollDelta.y * new Vector3(0,0,36));
        
        mouseObject.transform.position = new Vector3 (mousePosition.x , mousePosition.y, -5);
        mouseColor.a = 0.80f;
        mouseObject.GetComponent<SpriteRenderer>().color = mouseColor;
        //If we are in a collision, we don't need to check for glues so we just become translucent and return
        if(!mouseObject.GetComponent<CollisionDetection>().isPlaceable() || outsideCamera(mouseTile)){
            
            return;
        }

        //Point visualizer to check the math
        if(debug){
            int i = 0;
            foreach(Vector2 edge in mouseTile.getPoints()){
                visu[i].transform.position = new Vector3(edge.x, edge.y, -6f);
                i++;
            }
        }
        float minDist = float.MaxValue;
        Tile nearestTile = tiles[0];
        foreach(Tile t in tiles){
            Vector2[] edges = mouseTile.nearestEdgePair(t);
            float d = Vector2.Distance(edges[0],edges[1]);
            if( d < minDist && mouseTile.isCompatible(t)){
                minDist = d;
                nearestTile = t;
            }
        }
        //Vector2 pos = nearestTile.nearestEdge(mousePosition);
        //visu[0].transform.position = new Vector3(pos.x, pos.y, -6f);
        if(minDist < searchDistance){
            isGlued = mouseTile.Glue(nearestTile);
            if(isGlued){
                //Else we make sure we're solid color and check for the closest glue
                mouseColor.a = 1f;
                mouseObject.GetComponent<SpriteRenderer>().color = mouseColor;
                List<Tile> neighbours =  getNeighbours(mouseTile);
                foreach(Tile n in neighbours){
                    Vector3 diff = (Vector4) (n.getColor() - mouseColor);
                    mouseScore += scoreCalc(diff);
                }
            }   
        }
        else{
            isGlued = false;
        }
    }

    int scoreCalc(Vector3 color){
        float diff = color.sqrMagnitude;
        float maxDiff = 1.3f;
        float t = 1-(diff/maxDiff);
        int r = (int)Mathf.LerpUnclamped(0,1000,t);
        //Debug.Log(r);
        return Mathf.Max(0,r);
    }
    void LateUpdate(){
        if(endgame || !activeGame) return;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetCamSize, Time.deltaTime*5e-2f);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, camPosTarget, Time.deltaTime*camMoveSpeed);
        //mouseScoreText.GetComponent<TextMeshProUGUI>().text = $"{mouseScore}";
        if(Input.GetMouseButtonUp(0) && isGlued){
                Color c = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                Vector3 pos = new Vector3(mouseObject.transform.position.x, mouseObject.transform.position.y, 0f);
                mouseColor.a = 1f;
                Tile t = addTile(pos, mouseObject.transform.rotation.eulerAngles.z, mouseColor);
                Vector2[] points = t.getPoints();
                float camFactor = 0.8f; //controls how much border we should leave
                foreach(Vector2 p in points){
                    if(Mathf.Abs(p.x) > camWidth/2*camFactor || Mathf.Abs(p.y) > camHeight/2*camFactor){
                        targetCamSize = Mathf.Min(targetCamSize+0.3f,6.5f);
                        break;
                    }
                }
                List<Tile> neighbours =  getNeighbours(t);
                foreach(Tile n in neighbours){
                    Vector3 diff = (Vector4) (n.getColor() - t.getColor());
                    score += scoreCalc(diff);
                }
                scoreText.GetComponent<TextMeshProUGUI>().text = $"{score}";

                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.pitch = mouseColor.r + mouseColor.b;
                audioSource.Play();

                mouseColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                mouseTile.setColor(mouseColor); 
                
            
            foreach(Vector2 p in t.getPoints()){
                if(p.x > north) north = p.x;
                if(p.x < south) south = p.x;
                if(p.y > west) west = p.y;
                if(p.y <east) east = p.y;
            }
            

            camPosTarget = new Vector3(north+south,east+west,-20)/2;

                
            }
    }

    Pentagon addPentagon(Vector3 pos, float rot, Color color, bool mouseTile = false){
        GameObject go = Instantiate(pentagon, pos, Quaternion.Euler(0,0,rot));
        Pentagon p = new Pentagon(go);
        p.setColor(color);

        if(mouseTile){
            go.name = "Mouse Tile";
            return p;
        }

        go.transform.SetParent(parent.transform);
        tiles.Add(p);
        setRandomMouseTileType();
        return p;
    }

    Star addStar(Vector3 pos, float rot, Color color, bool mouseTile = false){
        GameObject go = Instantiate(star, pos, Quaternion.Euler(0,0,rot));
        Star s = new Star(go);
        s.setColor(color);

        if(mouseTile){
            go.name = "Mouse Tile";
            return s;
        }
        
        go.transform.SetParent(parent.transform);
        setRandomMouseTileType();
        tiles.Add(s);
        
        return s;
    }

    Boat addBoat(Vector3 pos, float rot, Color color, bool mouseTile = false){
        GameObject go = Instantiate(boat, pos, Quaternion.Euler(0,0,rot));
        Boat s = new Boat(go);
        s.setColor(color);

        if(mouseTile){
            go.name = "Mouse Tile";
            return s;
        }
        
        go.transform.SetParent(parent.transform);
        setRandomMouseTileType();
        tiles.Add(s);
        
        return s;
    }

    Rhombus addRhombus(Vector3 pos, float rot, Color color, bool mouseTile = false){
        GameObject go = Instantiate(rhombus, pos, Quaternion.Euler(0,0,rot));
        Rhombus s = new Rhombus(go);
        s.setColor(color);

        if(mouseTile){
            go.name = "Mouse Tile";
            return s;
        }
        
        go.transform.SetParent(parent.transform);
        setRandomMouseTileType();
        tiles.Add(s);
        
        return s;
    }

    float getMouseAngle(){
        return Mathf.Round(mouseObject.transform.rotation.eulerAngles.z);
    }

    
}
