using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Gondola : MonoBehaviour
{

    /// <summary>
    /// Shelf items MUST be smaller than the height of the shelf. 
    /// </summary>
    public GameObject[] shelf_items;
    public GameObject cereal;


    /// <summary>
    /// This needs to be changed if the model changes to have more shelves. 
    /// </summary>


    private BoxCollider[] shelf_locations;

    /// <summary>
    /// WARNING: This should only be true in ONE gondola, or it will spawn multiple cereals thus breaking the game. 
    /// </summary>
    public bool mustSpawnCereal { get; set; } = false; 
    public bool cerealSpawnTarget = false;

    public static List<GameObject> cerealTargets = new List<GameObject>();

    private Vector3 shelf_dimensions;
    public int items_per_shelf = 5;
    public int num_shelves = 3;

    private void Awake()
    {
        shelf_locations = GetItemShelves(this.gameObject);
        if (cerealSpawnTarget) cerealTargets.Add(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // since the shelf is mostly a box shape, we can just use the renderer dimensions
        shelf_dimensions = GetComponent<Renderer>().bounds.size;
        if(!cerealSpawnTarget) PlaceShelfItemsV2();

    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaceShelfItemsV2()
    {
        int cereal_shelf = -1;
        if(mustSpawnCereal && cerealSpawnTarget) cereal_shelf = Random.Range(0, num_shelves);
        

        for (int i = 0; i < num_shelves; i++)
        {
            Transform s_start = this.gameObject.transform.Find("s" + (i+1) + "_start");
            Transform s_end = this.gameObject.transform.Find("s" + (i+1) + "_end");

            if (i == cereal_shelf) PlaceCerealOnShelf(s_start, s_end);
            else PlaceShelfObjects(s_start, s_end);
           
        }
    }

    public void PlaceShelfObjects(Transform s_start, Transform s_end)
    {
        int randIndex = Random.Range(0, shelf_items.Length);
        float shelf_width = s_end.position.x - s_start.position.x;

        for (int j = 0; j < items_per_shelf; j++)
        {
            GameObject obj = Instantiate(shelf_items[randIndex]);
            Vector3 objSize = obj.GetComponent<Renderer>().bounds.size;

            float xPos = (shelf_width) * (j / (float)items_per_shelf) + (objSize.x / 2f);
            float zPos = (s_end.position.z - s_start.position.z) * (j / (float)items_per_shelf); //need to interpolate 


            obj.transform.position = s_start.position + new Vector3(xPos, 0, zPos);
            //the gondolas are rotated 90 degrees lol, so we need to undo that 
            obj.transform.localEulerAngles = gameObject.transform.localEulerAngles + new Vector3(-gameObject.transform.localEulerAngles.x, 0, 0); 
        }
    }

    public void PlaceCerealOnShelf(Transform s_start, Transform s_end)
    {
        float shelf_width = s_end.position.x - s_start.position.x;

        GameObject obj = Instantiate(cereal);
        obj.name = "Cereal";
        Vector3 objSize = obj.GetComponent<Renderer>().bounds.size;

        float xPos = (shelf_width/2f)+ (objSize.x / 2f);
        float zPos = (s_end.position.z - s_start.position.z)/2f; //need to interpolate 

        obj.transform.position = s_start.position + new Vector3(xPos, 0, zPos);
        //the gondolas are rotated 90 degrees lol, so we need to undo that 
        obj.transform.localEulerAngles = gameObject.transform.localEulerAngles + new Vector3(-gameObject.transform.localEulerAngles.x, 0, 0);

    }


    public void PlaceShelfItems()
    {
        for (int i = 0; i < shelf_locations.Length; i++) {
            BoxCollider shelf = shelf_locations[i];
            float shelf_width = shelf.bounds.extents.x;// * gameObject.transform.localScale.x;
            float shelf_depth = shelf.bounds.extents.z;// * gameObject.transform.localScale.y;
            float shelf_height = shelf.bounds.extents.y;// * gameObject.transform.localScale.z;

            Debug.Log("Bounds: " + shelf.bounds.ToString());

            int randIndex = i % shelf_items.Length;//Random.Range(0, shelf_items.Length);

            for (int j = 0; j < items_per_shelf; j++)
            {
               
                GameObject obj = Instantiate(shelf_items[randIndex]);
                Vector3 objSize = obj.GetComponent<Renderer>().bounds.size;
                //objSize.Scale(obj.transform.localScale);

                Vector3 adjustedCenter = shelf.center;
                adjustedCenter.Scale(gameObject.transform.localScale);

                //since the object is rotated 90 degrees (THUOHNSUEONTEU)
                float t = adjustedCenter.y;
                adjustedCenter.y = adjustedCenter.z;
                adjustedCenter.z = t;

                //Debug.Log(objSize);
                //Debug.Log(shelf_width + " " + shelf_depth + " " + shelf_height);

                adjustedCenter.x += (-shelf_width / objSize.x) * (j / (float)items_per_shelf) - (shelf_width / 2f) + (objSize.x / 2f);
                //adjustedCenter.y += shelf_height /2f + objSize.y/2f;
                adjustedCenter.z += -shelf_depth /4f + objSize.z/2f;


                //adjustedCenter = sh;
                Debug.Log(adjustedCenter);


                obj.transform.position = adjustedCenter;
                //obj.transform.position.Scale(this.gameObject.transform.localScale);
            }
        }
    }

    /// <summary>
    /// Finds box colliders for shelves via the shelf physics material. Box colliders suck though...
    /// </summary>
    /// <param name="shelfObj"></param>
    /// <returns></returns>
    public BoxCollider[] GetItemShelves(GameObject shelfObj)
    {
        BoxCollider[] boxColliders = shelfObj.GetComponents<BoxCollider>();
        List<BoxCollider> boxes = new List<BoxCollider>();
       
        for (int i = 0; i < boxColliders.Length; i++)
        {
            if(boxColliders[i].material.name.Contains("Shelf"))
            {
                boxes.Add(boxColliders[i]);
            }
        }

        return boxes.ToArray();
    }

}
