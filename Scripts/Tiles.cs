using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 

public abstract class Tile{
    //not used, but helpful to figure out geometry
    public static float PentagonSide = 0.6498f, PentagonHeight = 1f, PentagonOuterRadius = 0.5528f, PentagonInnerRadius = 0.4472f;
    public static float scale = 1f;

    public GameObject go;
    public tileTypes type;
    public enum tileTypes {
        Pentagon,
        Star,
        Boat,
        Rhombus
    };

    

    public static Vector2 rotate(Vector2 v, float delta) {
        delta *= Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
    
    public bool Glue(Tile t){
        //tries to glue this tile to t
        /*Vector2[] pair = t.nearestPair(this);

        this.go.transform.position -= new Vector3 (pair[1].x - pair[0].x, pair[1].y - pair[0].y, 0);*/

        //Vector2 thisEdge = this.nearestEdge(t);
        //Vector2 tEdge    = t.nearestEdge(this);
        Vector2[] edgePair = nearestEdgePair(t);
        Vector3 newPos = this.go.transform.position - new Vector3 (edgePair[0].x - edgePair[1].x, edgePair[0].y - edgePair[1].y, 0);
        Vector2[] points = this.getPoints(newPos);
        Vector2[] edges = this.getEdges(newPos);
        if(Physics2D.OverlapPointAll(newPos).Length > 0 && Physics2D.OverlapPointAll(newPos)[0] != go.GetComponent<Collider2D>()){
                return false;
            }
        foreach(Vector2 p in points){
            if(Physics2D.OverlapPointAll(p).Length > 0){
                return false;
            }
        }
        foreach(Vector2 p in edges){
            if(Physics2D.OverlapPointAll(p).Length > 0){
                return false;
            }
        }
        this.go.transform.position = newPos;
        return true;
    }

    
    
    public float getDistance(Tile t){
        return Vector2.Distance(this.getCenter(), t.getCenter());
    }

    public float getDistance(Vector2 pos){
        return Vector2.Distance(this.getCenter(), pos);
    }

    public float getRotation(){
        return Mathf.Round(go.transform.rotation.eulerAngles.z);
    }

    public Vector2 getCenter(){
        return (Vector2) go.transform.position;
    }
    
    public void setColor(Color c){
        go.GetComponent<SpriteRenderer>().color = c;
    }

    public Color getColor(){
         return go.GetComponent<SpriteRenderer>().color;
    }


    abstract public bool isCompatible(Tile t);
    public Vector2[] getPoints(){
        return getPoints(this.getCenter());
    }
    abstract public Vector2[] getPoints(Vector2 p);
    abstract public bool checkCollision(Vector2[] points);

    public Vector2[] getEdges(Vector2 center){
        Vector2[] points = this.getPoints(center);
        int l = points.Length;
        Vector2[] edges = new Vector2[l];
        for(int i =0; i < l; i++){
            edges[i] = points[i]*0.5f + points[(i+1)%l]*0.5f;
        }

        return edges;
    }

    public Vector2[] getEdges(){
        return getEdges(this.getCenter());
    }

    public Vector2 nearestEdge(Vector2 pos){
        float minDist = float.MaxValue;
        Vector2[] edges = this.getEdges();
        Vector2 nearEdge = edges[0];
        foreach(Vector2 e in edges){
            float dist = Vector2.Distance(pos,e);
            if(dist < minDist){
                minDist = dist;
                nearEdge = e;
            }
        }

        return nearEdge;
    }

    public Vector2 nearestEdge(Tile t){
        return nearestEdge(t.getCenter());
    }


    public Vector2[] nearestEdgePair(Tile t){
        //returns an array
        //First element is the edge closest to this tile
        //Second element is the corresponding edge on the other tile
        float maxAngle = 1f;
        Vector2[] edges = this.getEdges();
        Vector2[] points = this.getPoints();

        Vector2[] tEdges = t.getEdges();
        Vector2[] tPoints = t.getPoints();

        int nearest = 0, tNearest = 0;

        float minDist = float.MaxValue;
        for(int i = 0; i < edges.Length; i++){
            float dist = Vector2.Distance(t.getCenter(),edges[i]);
            if(dist < minDist){
                minDist = dist;
                nearest = i;
            }
        }

        Vector2 vector = edges[nearest] - points[nearest];
        minDist = float.MaxValue;
        for(int i = 0; i < tEdges.Length; i++){
            float dist = Vector2.Distance(this.getCenter(),tEdges[i]);
            Vector2 tVector = tEdges[i] - tPoints[i];
            if(dist < minDist && Vector2.Angle(vector, tVector)%180 < maxAngle){
                
                minDist = dist;
                tNearest = i;
            }
        }
        return new Vector2[] {edges[nearest], tEdges[tNearest]};
    }

    public Vector2[] nearestPair(Tile t){
        float minDist = float.MaxValue;
        Vector2[] points = this.getPoints();
        Vector2[] nearPoints = new Vector2[2];

        //Step 1 : grab the nearest point to the center
        foreach (Vector2 point in points){
            float dist = Vector2.Distance(t.getCenter(),point);
            if (dist < minDist){
                nearPoints[0] = point;
                minDist = dist;
            }
        }
        minDist = float.MaxValue;
        //Step 2 : check which point on this other side is the nearest to this point
        foreach(Vector2 point in t.getPoints()){
            float dist = Vector2.Distance(nearPoints[0], point);
            if (dist < minDist){
                nearPoints[1] = point;
                minDist = dist;
            }
        }
        return nearPoints;
    }
    
}

public class Pentagon : Tile{

    public Pentagon(GameObject go){
        this.go = go;
        this.type = tileTypes.Pentagon;
    }
    
    public override bool isCompatible(Tile t){
        float rotDiff = this.getRotation() - t.getRotation();
        
        if(t.type == tileTypes.Pentagon && (Mathf.Abs(rotDiff/36)%2) == 1){
            return true;
        }
        else if(t.type == tileTypes.Star && (Mathf.Abs(rotDiff/36)%2) == 0){
            return true;
        }

        return true;
    }

    public override bool checkCollision(Vector2[] points){
        foreach(Vector2 p in points){
            if(Vector2.Distance(getCenter(), p) < PentagonInnerRadius){
                return true;
            }
        }
        return false;
    }

    
    public override Vector2[] getPoints(Vector2 center){
        float circumCircle = 0.5528f;
        float rot = this.getRotation();
        Vector2[] points = new Vector2[5];
        float s1 = Mathf.Sin(18*Mathf.Deg2Rad)*circumCircle;
        float s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*circumCircle;
        float c1 = Mathf.Cos(18*Mathf.Deg2Rad)*circumCircle;
        float c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*circumCircle;
        points[0] = center + rotate(new Vector2(-c2,s2),rot)*scale;
        points[1] = center + rotate(new Vector2(c2, s2),rot)*scale;
        points[2] = center + rotate(new Vector2(c1,s1),rot)*scale;
        points[3] = center + rotate(new Vector2(0f, circumCircle),rot)*scale;
        points[4] = center + rotate(new Vector2(-c1,s1),rot)*scale;

        return points;
    }

}

public class Star : Tile{
    static float innerRadius = 0.3416f;
    static float outerRadius = 0.894f;

    public Star(GameObject go){
        this.go = go;
        this.type = tileTypes.Star;
    }
    
    public override Vector2[] getPoints(Vector2 center){
        
        Vector2[] points = new Vector2[10];
        float rot = this.getRotation();
        
        float s1 = Mathf.Sin(18*Mathf.Deg2Rad)*outerRadius;
        float s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*outerRadius;
        float c1 = Mathf.Cos(18*Mathf.Deg2Rad)*outerRadius;
        float c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*outerRadius;
        points[0] = center + rotate(new Vector2(-c2,s2),rot)*scale;
        points[2] = center + rotate(new Vector2(c2, s2),rot)*scale;
        points[4] = center + rotate(new Vector2(c1,s1),rot)*scale;
        points[6] = center + rotate(new Vector2(0f, outerRadius),rot)*scale;
        points[8] = center + rotate(new Vector2(-c1,s1),rot)*scale;

        s1 = Mathf.Sin(18*Mathf.Deg2Rad)*innerRadius;
        s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*innerRadius;
        c1 = Mathf.Cos(18*Mathf.Deg2Rad)*innerRadius;
        c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*innerRadius;

        rot += 36;

        points[1] = center + rotate(new Vector2(-c2,s2),rot)*scale;
        points[3] = center + rotate(new Vector2(c2, s2),rot)*scale;
        points[5] = center + rotate(new Vector2(c1,s1),rot)*scale;
        points[7] = center + rotate(new Vector2(0f, innerRadius),rot)*scale;
        points[9] = center + rotate(new Vector2(-c1,s1),rot)*scale;


        return points;

    }

    public override bool isCompatible(Tile t){
        float rotDiff = this.getRotation() - t.getRotation();
        
        if(t.type == tileTypes.Pentagon && (Mathf.Abs(rotDiff/36)%2) == 0){
            return true;
        }
        else if(t.type == tileTypes.Star && (Mathf.Abs(rotDiff/36)%2) == 1){
            return true;
        }

        return true;
    }

    public override bool checkCollision(Vector2[] points){
        foreach(Vector2 p in points){
            if(Vector2.Distance(getCenter(), p) < innerRadius*0.9f){
                return true;
            }
        }
        return false;
    }
}

public class Boat : Tile{

    public Boat(GameObject go){
        this.go = go;
        this.type = tileTypes.Boat;
    }

    public override Vector2[] getPoints(Vector2 center){
        float innerRadius = 0.3416f;
        float outerRadius = 0.894f;
        Vector2[] points = new Vector2[7];
        float rot = this.getRotation();
        
        float s1 = Mathf.Sin(18*Mathf.Deg2Rad)*outerRadius;
        float s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*outerRadius;
        float c1 = Mathf.Cos(18*Mathf.Deg2Rad)*outerRadius;
        float c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*outerRadius;
        points[2] = center + rotate(new Vector2(c1,s1),rot)*scale;
        points[4] = center + rotate(new Vector2(0f, outerRadius),rot)*scale;
        points[6] = center + rotate(new Vector2(-c1,s1),rot)*scale;

        s1 = Mathf.Sin(18*Mathf.Deg2Rad)*innerRadius;
        s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*innerRadius;
        c1 = Mathf.Cos(18*Mathf.Deg2Rad)*innerRadius;
        c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*innerRadius;

        rot += 36;

        points[1] = center + rotate(new Vector2(c2, s2),rot)*scale;
        points[3] = center + rotate(new Vector2(c1,s1),rot)*scale;
        points[5] = center + rotate(new Vector2(0f, innerRadius),rot)*scale;
        points[0] = center + rotate(new Vector2(-c1,s1),rot)*scale;

        return points;

    }

    public override bool isCompatible(Tile t){
        return true;
    }

    public override bool checkCollision(Vector2[] points){
        return false;
    }
}

public class Rhombus : Tile{

    static float height = PentagonSide * Mathf.Cos(54*Mathf.Deg2Rad);
    static float bias = PentagonSide * Mathf.Sin(54*Mathf.Deg2Rad);
    static float width = PentagonSide + bias;
    public Rhombus(GameObject go){
        this.go = go;
        this.type = tileTypes.Rhombus;
    }

    public override Vector2[] getPoints(Vector2 center){
        /*float innerRadius = 0.3416f;
        float outerRadius = 0.894f;
        Vector2[] points = new Vector2[7];
        Vector2 center = this.getCenter();
        float rot = this.getRotation();
        
        float s1 = Mathf.Sin(18*Mathf.Deg2Rad)*outerRadius;
        float s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*outerRadius;
        float c1 = Mathf.Cos(18*Mathf.Deg2Rad)*outerRadius;
        float c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*outerRadius;

        points[2] = center + rotate(new Vector2(-c1,s1),rot);

        s1 = Mathf.Sin(18*Mathf.Deg2Rad)*innerRadius;
        s2 = Mathf.Sin(-54*Mathf.Deg2Rad)*innerRadius;
        c1 = Mathf.Cos(18*Mathf.Deg2Rad)*innerRadius;
        c2 = Mathf.Cos(-54*Mathf.Deg2Rad)*innerRadius;

        rot += 36;

        points[1] = center + rotate(new Vector2(c2, s2),rot);
        points[3] = center + rotate(new Vector2(0f, innerRadius),rot);
        points[0] = center + rotate(new Vector2(-c1,s1),rot);
        */
        Vector2[] points = new Vector2[4];
        float rot = this.getRotation();
        

        points[0] = center + rotate(new Vector2(-width/2+bias, -height/2),rot)*scale;
        points[1] = center + rotate(new Vector2(width/2, -height/2),rot)*scale;
        points[2] = center + rotate(new Vector2(width/2-bias, height/2),rot)*scale;
        points[3] = center + rotate(new Vector2(-width/2, height/2),rot)*scale;
        return points;


    }

    public override bool isCompatible(Tile t){
        if(t.type != Tile.tileTypes.Rhombus){
            return true;
        }
        if(t.getRotation()%180 - this.getRotation()%180 <= 36){
            return true;
        }

        return false;
    }

    public override bool checkCollision(Vector2[] points){
        return false;
    }
}