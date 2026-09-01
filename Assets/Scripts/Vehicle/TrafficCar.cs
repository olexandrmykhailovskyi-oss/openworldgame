using System.Collections.Generic;
using UnityEngine;

namespace OpenWorld
{
    public class TrafficCar : MonoBehaviour
    {
        public float maxSpeed = 10f;
        public float turnSpeedDeg = 110f;
        public float laneOffset = 2.6f;
        public float reachDistance = 2.5f;
        public float obstacleDistance = 6f;

        Transform[] wheels;
        CityGenerator city;
        Vector2Int targetIntersection;
        Vector2Int dir;
        Vector3 targetPoint;
        float speed;
        System.Random rnd;

        public static TrafficCar Create(CityGenerator city, Vector2Int intersection, Vector2Int direction, Color color)
        {
            var root = new GameObject("TrafficCar");
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.75f, 0f);
            col.size = new Vector3(1.9f, 1.5f, 4.4f);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            body.transform.localScale = new Vector3(1.9f, 0.7f, 4.4f);
            body.GetComponent<MeshRenderer>().sharedMaterial = Material(color);

            var cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 1.32f, -0.2f);
            cabin.transform.localScale = new Vector3(1.7f, 0.55f, 2.2f);
            cabin.GetComponent<MeshRenderer>().sharedMaterial = Material(new Color(0.12f, 0.13f, 0.16f));

            var wheels = new Transform[4];
            Vector3[] wpos =
            {
                new Vector3(-0.95f, 0.38f, 1.45f),
                new Vector3(0.95f, 0.38f, 1.45f),
                new Vector3(-0.95f, 0.38f, -1.45f),
                new Vector3(0.95f, 0.38f, -1.45f)
            };
            for (int i = 0; i < 4; i++)
            {
                wheels[i] = CreateWheel(root.transform, wpos[i]);
            }

            var car = root.AddComponent<TrafficCar>();
            car.wheels = wheels;
            car.Setup(city, intersection, direction);
            return car;
        }

        static Transform CreateWheel(Transform parent, Vector3 localPos)
        {
            var pivot = new GameObject("WheelMesh");
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = localPos;

            var tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tire.name = "Tire";
            var tireCol = tire.GetComponent<CapsuleCollider>();
            if (tireCol != null) Destroy(tireCol);
            tire.transform.SetParent(pivot.transform, false);
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tire.transform.localScale = new Vector3(0.76f, 0.13f, 0.76f);
            tire.GetComponent<MeshRenderer>().sharedMaterial = Material(new Color(0.08f, 0.08f, 0.08f));

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rim.name = "Rim";
            var rimCol = rim.GetComponent<BoxCollider>();
            if (rimCol != null) Destroy(rimCol);
            rim.transform.SetParent(pivot.transform, false);
            rim.transform.localScale = new Vector3(0.82f, 0.6f, 0.1f);
            rim.GetComponent<MeshRenderer>().sharedMaterial = Material(new Color(0.55f, 0.55f, 0.58f));

            return pivot.transform;
        }

        public void Setup(CityGenerator gen, Vector2Int intersection, Vector2Int direction)
        {
            city = gen;
            targetIntersection = intersection;
            dir = direction;
            rnd = new System.Random(GetInstanceID());
            targetPoint = ApproachPoint(targetIntersection, dir);
            transform.position = new Vector3(targetPoint.x, 0f, targetPoint.z);
            transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y), Vector3.up);
        }

        void FixedUpdate()
        {
            if (city == null) return;

            Vector3 origin = transform.position + Vector3.up * 0.8f + transform.forward * 2.6f;
            bool obstacle = Physics.Raycast(origin, transform.forward, obstacleDistance);
            float targetSpeed = obstacle ? 0f : maxSpeed;
            speed = Mathf.MoveTowards(speed, targetSpeed, 8f * Time.deltaTime);

            if (speed > 0.01f)
                transform.position += transform.forward * speed * Time.deltaTime;

            Vector3 toTarget = targetPoint - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude < reachDistance)
            {
                PickNext();
                toTarget = targetPoint - transform.position;
                toTarget.y = 0f;
            }

            if (toTarget.sqrMagnitude > 0.001f)
            {
                var look = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeedDeg * Time.deltaTime);
            }

            SpinWheels();
        }

        void PickNext()
        {
            var options = new List<Vector2Int>();
            AddOption(options, dir, 3);
            AddOption(options, new Vector2Int(dir.y, -dir.x), 1);
            AddOption(options, new Vector2Int(-dir.y, dir.x), 1);

            if (options.Count == 0) dir = -dir;
            else dir = options[rnd.Next(options.Count)];

            targetIntersection += dir;
            targetPoint = ApproachPoint(targetIntersection, dir);
        }

        void AddOption(List<Vector2Int> list, Vector2Int d, int weight)
        {
            var next = targetIntersection + d;
            if (next.x < 0 || next.x > city.blocksX || next.y < 0 || next.y > city.blocksZ) return;
            for (int i = 0; i < weight; i++) list.Add(d);
        }

        Vector3 ApproachPoint(Vector2Int ix, Vector2Int d)
        {
            Vector3 c = new Vector3(city.RoadLineX(ix.x), 0f, city.RoadLineZ(ix.y));
            Vector3 d3 = new Vector3(d.x, 0f, d.y);
            Vector3 right = new Vector3(d.y, 0f, -d.x);
            return c - d3 * 5f + right * laneOffset;
        }

        void SpinWheels()
        {
            if (wheels == null) return;
            float deg = speed * Time.deltaTime * 150f;
            var rot = Vector3.right * deg;
            for (int i = 0; i < wheels.Length; i++)
                if (wheels[i] != null) wheels[i].Rotate(rot, Space.Self);
        }

        static Material Material(Color c)
        {
            var m = new Material(Shader.Find("Standard"));
            if (m != null) m.color = c;
            return m;
        }
    }
}
