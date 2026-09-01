using UnityEngine;

namespace OpenWorld
{
    public static class CarFactory
    {
        public static CarController Create(Vector3 position, float yaw, Color color)
        {
            var root = new GameObject("Car");
            root.tag = "Car";
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            var rb = root.AddComponent<Rigidbody>();
            rb.mass = 1500f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

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

            var wcols = new WheelCollider[4];
            var meshes = new Transform[4];
            Vector3[] wpos =
            {
                new Vector3(-0.95f, 0.38f, 1.45f),
                new Vector3(0.95f, 0.38f, 1.45f),
                new Vector3(-0.95f, 0.38f, -1.45f),
                new Vector3(0.95f, 0.38f, -1.45f)
            };

            for (int i = 0; i < 4; i++)
            {
                var wgo = new GameObject("WheelCollider_" + i);
                wgo.transform.SetParent(root.transform, false);
                wgo.transform.localPosition = wpos[i];
                var wc = wgo.AddComponent<WheelCollider>();
                SetupWheel(wc);
                wcols[i] = wc;

                var pivot = new GameObject("WheelMesh_" + i);
                pivot.transform.SetParent(root.transform, false);
                pivot.transform.localPosition = wpos[i];

                var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cyl.name = "Wheel";
                var cylCol = cyl.GetComponent<CapsuleCollider>();
                if (cylCol != null) Object.Destroy(cylCol);
                cyl.transform.SetParent(pivot.transform, false);
                cyl.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                cyl.transform.localScale = new Vector3(0.76f, 0.13f, 0.76f);
                cyl.GetComponent<MeshRenderer>().sharedMaterial = Material(new Color(0.08f, 0.08f, 0.08f));

                var rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rim.name = "Rim";
                var rimCol = rim.GetComponent<BoxCollider>();
                if (rimCol != null) Object.Destroy(rimCol);
                rim.transform.SetParent(pivot.transform, false);
                rim.transform.localScale = new Vector3(0.82f, 0.6f, 0.1f);
                rim.GetComponent<MeshRenderer>().sharedMaterial = Material(new Color(0.55f, 0.55f, 0.58f));

                meshes[i] = pivot.transform;
            }

            var car = root.AddComponent<CarController>();
            car.wheels = wcols;
            car.wheelMeshes = meshes;
            return car;
        }

        static void SetupWheel(WheelCollider wc)
        {
            wc.radius = 0.38f;
            wc.suspensionDistance = 0.3f;
            wc.mass = 20f;
            var spring = wc.suspensionSpring;
            spring.spring = 35000f;
            spring.damper = 4500f;
            spring.targetPosition = 0.5f;
            wc.suspensionSpring = spring;

            var fwd = wc.forwardFriction;
            fwd.stiffness = 1.2f;
            wc.forwardFriction = fwd;

            var side = wc.sidewaysFriction;
            side.stiffness = 1.5f;
            wc.sidewaysFriction = side;
        }

        static Material Material(Color c)
        {
            var m = new Material(Shader.Find("Standard"));
            if (m != null) m.color = c;
            return m;
        }
    }
}
