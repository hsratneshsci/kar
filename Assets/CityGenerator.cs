using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    [Header("City Settings")]
    public int numberOfBuildings = 200;
    public Vector2 citySize = new Vector2(100f, 100f);
    public float minExtraGap = 5f;

    [Header("Building Settings")]
    public Vector2 buildingWidthRange = new Vector2(3f, 8f);
    public Vector2 buildingHeightRange = new Vector2(5f, 30f);
    public GameObject buildingPrefab;

    [Header("Traffic Settings")]
    public GameObject carPrefab;
    public GameObject trafficLightPrefab;
    public int numberOfCars = 10;

    private List<BuildingData> placedBuildings = new List<BuildingData>();
    private List<GameObject> cars = new List<GameObject>();

    private Material policeMaterial, hospitalMaterial, fireStationMaterial, houseMaterial, parkMaterial, randomMaterial;
    private Material roadMaterial;

    private float roadWidth = 2f;
    private float roadSpacing = 30f;

    private enum BuildingType { Police, Hospital, FireStation, House, Park, Random }

    private struct BuildingData
    {
        public Vector3 position;
        public Vector3 size;
    }

    void Start()
    {
        CreateMaterials();
        GenerateCity();
        SpawnCars();
    }

    void Update()
    {
        MoveCars();
    }

    void CreateMaterials()
    {
        policeMaterial = CreateColorMaterial(Color.blue);
        hospitalMaterial = CreateColorMaterial(Color.red);
        fireStationMaterial = CreateColorMaterial(new Color(1f, 0.5f, 0f));
        houseMaterial = CreateColorMaterial(Color.white);
        parkMaterial = CreateColorMaterial(Color.green);
        randomMaterial = CreateColorMaterial(Random.ColorHSV());
        roadMaterial = CreateColorMaterial(new Color(0.1f, 0.1f, 0.1f));
    }

    Material CreateColorMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 0.3f);
        return mat;
    }

    void GenerateCity()
    {
        int attempts = 0;
        int maxAttempts = numberOfBuildings * 20;

        while (placedBuildings.Count < numberOfBuildings && attempts < maxAttempts)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-citySize.x / 2f, citySize.x / 2f),
                0f,
                Random.Range(-citySize.y / 2f, citySize.y / 2f)
            );

            float width = Random.Range(buildingWidthRange.x, buildingWidthRange.y);
            float depth = Random.Range(buildingWidthRange.x, buildingWidthRange.y);
            float height = Random.Range(buildingHeightRange.x, buildingHeightRange.y);

            Vector3 buildingSize = new Vector3(width, height, depth);

            if (IsSpaceAvailable(randomPos, buildingSize))
            {
                BuildingData data = new BuildingData
                {
                    position = randomPos,
                    size = buildingSize
                };
                placedBuildings.Add(data);

                GameObject building = Instantiate(buildingPrefab, randomPos, Quaternion.identity, transform);
                building.transform.localScale = new Vector3(width, height, depth);
                building.transform.Rotate(0f, Random.Range(0f, 360f), 0f);

                BuildingType type = (BuildingType)Random.Range(0, System.Enum.GetValues(typeof(BuildingType)).Length);
                AssignMaterial(building, type);
            }

            attempts++;
        }

        GenerateRoads();
        GenerateTrafficLights();
    }

    bool IsSpaceAvailable(Vector3 newPos, Vector3 newSize)
    {
        foreach (BuildingData building in placedBuildings)
        {
            float minDistanceX = (building.size.x / 2f) + (newSize.x / 2f) + minExtraGap;
            float minDistanceZ = (building.size.z / 2f) + (newSize.z / 2f) + minExtraGap;

            if (Mathf.Abs(newPos.x - building.position.x) < minDistanceX &&
                Mathf.Abs(newPos.z - building.position.z) < minDistanceZ)
            {
                return false;
            }
        }
        return true;
    }

    void AssignMaterial(GameObject building, BuildingType type)
    {
        Renderer rend = building.GetComponent<Renderer>();
        if (rend == null) return;

        switch (type)
        {
            case BuildingType.Police:
                rend.material = policeMaterial;
                break;
            case BuildingType.Hospital:
                rend.material = hospitalMaterial;
                break;
            case BuildingType.FireStation:
                rend.material = fireStationMaterial;
                break;
            case BuildingType.House:
                rend.material = houseMaterial;
                break;
            case BuildingType.Park:
                rend.material = parkMaterial;
                break;
            case BuildingType.Random:
                rend.material = randomMaterial;
                break;
        }
    }

    void GenerateRoads()
    {
        for (float x = -citySize.x / 2f; x <= citySize.x / 2f; x += roadSpacing)
        {
            Vector3 roadPos = new Vector3(x, 0f, 0f);
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.transform.parent = transform;
            road.transform.position = roadPos;
            road.transform.localScale = new Vector3(roadWidth, 0.1f, citySize.y);
            road.GetComponent<Renderer>().material = roadMaterial;
        }

        for (float z = -citySize.y / 2f; z <= citySize.y / 2f; z += roadSpacing)
        {
            Vector3 roadPos = new Vector3(0f, 0f, z);
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.transform.parent = transform;
            road.transform.position = roadPos;
            road.transform.localScale = new Vector3(citySize.x, 0.1f, roadWidth);
            road.GetComponent<Renderer>().material = roadMaterial;
        }
    }

    void GenerateTrafficLights()
    {
        for (float x = -citySize.x / 2f; x <= citySize.x / 2f; x += roadSpacing)
        {
            for (float z = -citySize.y / 2f; z <= citySize.y / 2f; z += roadSpacing)
            {
                Vector3 pos = new Vector3(x, 1f, z);
                GameObject light = Instantiate(trafficLightPrefab, pos, Quaternion.identity, transform);
                light.transform.localScale = new Vector3(0.5f, 2f, 0.5f);
            }
        }
    }

    void SpawnCars()
    {
        for (int i = 0; i < numberOfCars; i++)
        {
            bool horizontal = Random.value > 0.5f;
            float fixedPos = Random.Range(-citySize.x / 2f, citySize.x / 2f);

            Vector3 pos;
            Vector3 dir;
            if (horizontal)
            {
                pos = new Vector3(-citySize.x / 2f, 0.5f, fixedPos);
                dir = Vector3.right;
            }
            else
            {
                pos = new Vector3(fixedPos, 0.5f, -citySize.y / 2f);
                dir = Vector3.forward;
            }

            GameObject car = Instantiate(carPrefab, pos, Quaternion.identity, transform);
            car.AddComponent<CarMover>().direction = dir;
            cars.Add(car);
        }
    }

    void MoveCars()
    {
        foreach (GameObject car in cars)
        {
            if (car != null)
            {
                car.transform.Translate(car.GetComponent<CarMover>().direction * Time.deltaTime * 10f);

                if (Mathf.Abs(car.transform.position.x) > citySize.x / 2f + 10f ||
                    Mathf.Abs(car.transform.position.z) > citySize.y / 2f + 10f)
                {
                    car.transform.position = -car.GetComponent<CarMover>().direction * (Mathf.Max(citySize.x, citySize.y) / 2f);
                }
            }
        }
    }
}

public class CarMover : MonoBehaviour
{
    public Vector3 direction;
}
