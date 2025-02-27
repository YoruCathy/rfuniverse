using UnityEngine;
using RFUniverse.Attributes;
using System.Collections.Generic;
using Unity.Robotics.UrdfImporter.Control;
using Unity.Robotics.UrdfImporter;
using System.Linq;

namespace RFUniverse
{
    public static class RFUniverseUtility
    {
        public static Color EncodeIDAsColor(int instanceId)
        {
            long r = (instanceId * (long)16807 + 187) % 256;
            long g = (instanceId * (long)48271 + 79) % 256;
            long b = (instanceId * (long)95849 + 233) % 256;
            return new Color32((byte)r, (byte)g, (byte)b, 255);
        }

        public static List<TResult> GetChildComponentFilter<TResult>(this BaseAttr parent) where TResult : Component
        {
            return GetChildComponentFilter<BaseAttr, TResult>(parent);
        }

        public static List<TResult> GetChildComponentFilter<TParent, TResult>(TParent parent) where TParent : Component where TResult : Component
        {
            List<TResult> components = new List<TResult>();
            foreach (var item in parent.GetComponentsInChildren<TResult>())
            {
                if (item.GetComponentInParent<TParent>() == parent)
                    components.Add(item);
            }
            return components;
        }

        public static Transform FindChlid(this Transform parent, string targetName, bool includeSelf = true)
        {
            if (includeSelf && parent.name == targetName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = FindChlid(parent.GetChild(i), targetName, true);
                if (child == null)
                    continue;
                else
                    return child;
            }
            return null;
        }

        public static ArticulationUnit GetUnit(this ArticulationBody body)
        {
            if (body.TryGetComponent(out ArticulationUnit unit))
                return unit;
            else
                return body.gameObject.AddComponent<ArticulationUnit>();
        }
        public static ControllerAttr NormalizeRFUniverseArticulation(GameObject root)
        {
            ControllerAttr attr = root.GetComponent<ControllerAttr>() ?? root.AddComponent<ControllerAttr>();

            // Remove URDFImporter Scripts
            UrdfPlugins urdfPlugins = root.GetComponentInChildren<UrdfPlugins>();
            if (urdfPlugins != null)
                GameObject.DestroyImmediate(urdfPlugins.gameObject);
            Controller controller = root.GetComponentInChildren<Controller>();
            if (controller != null)
                Destroy(controller);
            UrdfRobot urdfRobot = root.GetComponentInChildren<UrdfRobot>();
            if (urdfRobot != null)
                Destroy(urdfRobot);
            UrdfLink[] urdfLinks = root.GetComponentsInChildren<UrdfLink>();
            foreach (var urdfLink in urdfLinks)
            {
                Destroy(urdfLink);
            }
            UrdfInertial[] urdfInertial = root.GetComponentsInChildren<UrdfInertial>();
            foreach (var item in urdfInertial)
            {
                Destroy(item);
            }
            UrdfJoint[] urdfJoint = root.GetComponentsInChildren<UrdfJoint>();
            foreach (var item in urdfJoint)
            {
                Destroy(item);
            }

            UrdfVisual[] urdfVisual = root.GetComponentsInChildren<UrdfVisual>();
            foreach (var item in urdfVisual)
            {
                Destroy(item);
            }

            UrdfCollision[] urdfCollision = root.GetComponentsInChildren<UrdfCollision>();
            foreach (var item in urdfCollision)
            {
                Destroy(item);
            }

            // Add basic script for root node
            IgnoreSelfCollision ign = root.GetComponent<IgnoreSelfCollision>() ?? root.AddComponent<IgnoreSelfCollision>();
            if (root.transform.GetChild(0).GetComponent<ArticulationBody>() == null)
                root.transform.GetChild(0).gameObject.AddComponent<ArticulationBody>();

            // Add RFUniverse scripts
            ArticulationBody[] articulationBodies = root.GetComponentsInChildren<ArticulationBody>();
            foreach (var body in articulationBodies)
            {
                if (body.gameObject.GetComponent<ArticulationUnit>() == null)
                {
                    body.gameObject.AddComponent<ArticulationUnit>();
                }
                if (body.isRoot)
                {
                    body.immovable = true;
                }

                var xDrive = body.xDrive;
                xDrive.stiffness = 100000;
                xDrive.damping = 9000;
                xDrive.forceLimit = float.MaxValue;
                body.xDrive = xDrive;

                var yDrive = body.yDrive;
                yDrive.stiffness = 100000;
                yDrive.damping = 9000;
                yDrive.forceLimit = float.MaxValue;
                body.yDrive = yDrive;

                var zDrive = body.zDrive;
                zDrive.stiffness = 100000;
                zDrive.damping = 9000;
                zDrive.forceLimit = float.MaxValue;
                body.zDrive = zDrive;

                List<Transform> renders = GetChildComponentFilter<ArticulationBody, Renderer>(body).Select((s) => s.transform).ToList();
                if (renders.Count == 0)
                    renders.Add(new GameObject("None").transform);
                foreach (var item in renders)
                {
#if UNITY_EDITOR
                    GameObject prefab = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(item.gameObject);
                    if (prefab != null)
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(prefab, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);
#endif
                    item.SetParent(body.transform);
                    item.SetSiblingIndex(0);
                }
                List<Collider> colliders = GetChildComponentFilter<ArticulationBody, Collider>(body);
                for (int i = 0; i < colliders.Count; i++)
                {
                    Collider collider = colliders[i];
#if UNITY_EDITOR
                    GameObject prefab = UnityEditor.PrefabUtility.GetNearestPrefabInstanceRoot(collider.gameObject);
                    if (prefab != null)
                        UnityEditor.PrefabUtility.UnpackPrefabInstance(prefab, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);
#endif
                    collider.name = "Collider";
                    int index = Mathf.Min(renders.Count - 1, i);
                    if (index >= 0 && index < renders.Count)
                        collider.transform.parent = renders[index];
                    else
                        collider.transform.parent = body.transform;
                }
            }

            UrdfVisuals[] urdfVisuals = root.GetComponentsInChildren<UrdfVisuals>();
            foreach (var item in urdfVisuals)
            {
                Destroy(item.gameObject);
            }
            UrdfCollisions[] urdfCollisions = root.GetComponentsInChildren<UrdfCollisions>();
            foreach (var item in urdfCollisions)
            {
                Destroy(item.gameObject);
            }

            attr.GetJointParameters();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(root);
#endif
            return attr;
        }

        public static void Destroy(Object obj)
        {
            if (Application.isEditor)
                GameObject.DestroyImmediate(obj);
            else
                GameObject.Destroy(obj);
        }

        public static List<BaseAttrData> SortByParent(List<BaseAttrData> datas)
        {
            List<int> headID = new List<int>();
            int i = 0;
            while (i < datas.Count)
            {
                if (datas[i].parentID > 0 && !headID.Contains(datas[i].parentID))
                {
                    datas.Remove(datas[i]);
                    datas.Add(datas[i]);
                }
                else
                {
                    headID.Add(datas[i].id);
                    i++;
                }
            }
            return datas;
        }

        public static List<Vector3> ListFloatToListVector3(List<float> floats)
        {
            List<Vector3> v3s = new List<Vector3>();
            int i = 0;
            while (i + 2 < floats.Count)
                v3s.Add(new Vector3(floats[i++], floats[i++], floats[i++]));
            return v3s;
        }
        public static List<Color> ListFloatToListColor(List<float> floats)
        {
            List<Color> v3s = new List<Color>();
            int i = 0;
            while (i + 2 < floats.Count)
                v3s.Add(new Color(floats[i++], floats[i++], floats[i++]));
            return v3s;
        }
        public static List<List<float>> ListFloatSlicer(List<float> floats, int count)
        {
            Queue<float> que = new Queue<float>(floats);
            List<List<float>> back = new();
            while (que.Count >= count)
            {
                List<float> one = new();
                for (int i = 0; i < count; i++)
                {
                    one.Add(que.Dequeue());
                }
                back.Add(one);
            }
            return back;
        }
        public static Matrix4x4 ListFloatToMatrix(List<float> floats)
        {
            Matrix4x4 matrix = new();
            for (int i = 0; i < 16; i++)
            {
                matrix[i] = floats[i];
            }
            return matrix;
        }
        public static List<float> MatrixToListFloat(Matrix4x4 matrix)
        {
            List<float> floats = new();
            for (int i = 0; i < 16; i++)
            {
                floats.Add(matrix[i]);
            }
            return floats;
        }
        public static float[,] MatrixToFloatArray(Matrix4x4 matrix)
        {
            float[,] floats = new float[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    floats[i, j] = matrix[i, j];
                }
            }
            return floats;
        }
        public static Matrix4x4 FloatArrayToMatrix(float[,] floats)
        {
            Matrix4x4 matrix = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = floats[i, j];
                }
            }
            return matrix;
        }
        public static Matrix4x4 DoubleArrayToMatrix(double[,] floats)
        {
            Matrix4x4 matrix = new();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = (float)floats[i, j];
                }
            }
            return matrix;
        }
        public static List<float> ListVector3ToListFloat(List<Vector3> v3s)
        {
            List<float> fs = new();
            foreach (var item in v3s)
            {
                fs.Add(item.x);
                fs.Add(item.y);
                fs.Add(item.z);
            }
            return fs;
        }
        public static List<List<float>> ListVector3ToListFloat3(List<Vector3> v3s)
        {
            List<List<float>> f = new();
            foreach (var item in v3s)
            {
                List<float> fs = new List<float>();
                fs.Add(item.x);
                fs.Add(item.y);
                fs.Add(item.z);
                f.Add(fs);
            }
            return f;
        }
        public static List<float[,]> ListMatrixToListFloatArray(List<Matrix4x4> ms)
        {
            List<float[,]> f = new();
            foreach (var item in ms)
            {
                f.Add(MatrixToFloatArray(item));
            }
            return f;
        }
        public static List<Matrix4x4> ListMatrixTRS(List<Vector3> positioins, List<Quaternion> rotatiobs, List<Vector3> scales = null)
        {
            if (scales == null)
            {
                scales = new();
                for (int i = 0; i < positioins.Count; i++)
                {
                    scales.Add(Vector3.one);
                }
            }
            List<Matrix4x4> ms = new();
            for (int i = 0; i < positioins.Count; i++)
            {
                ms.Add(Matrix4x4.TRS(positioins[i], rotatiobs[i], scales[i]));
            }
            return ms;
        }
        public static List<Vector3> ListVector3LocalToWorld(List<Vector3> v3s, Transform trans)
        {
            List<Vector3> world = new();
            foreach (var item in v3s)
            {
                world.Add(trans.TransformPoint(item));
            }
            return world;
        }
        public static List<Quaternion> ListQuaternionLocalToWorld(List<Quaternion> qs, Transform trans)
        {
            List<Quaternion> world = new();
            foreach (var item in qs)
            {
                world.Add(trans.rotation * item);
            }
            return world;
        }
        public static List<Quaternion> ListFloatToListQuaternion(List<float> floats)
        {
            List<Quaternion> qs = new();
            int i = 0;
            while (i + 3 < floats.Count)
                qs.Add(new Quaternion(floats[i++], floats[i++], floats[i++], floats[i++]));
            return qs;
        }
        public static List<float> ListQuaternionToListFloat(List<Quaternion> qs)
        {
            List<float> fs = new List<float>();
            foreach (var item in qs)
            {
                fs.Add(item.x);
                fs.Add(item.y);
                fs.Add(item.z);
                fs.Add(item.w);
            }
            return fs;
        }
        public static List<int> GetChildIndexQueue(this Transform transform, Transform child)
        {
            if (!child.GetComponentsInParent<Transform>().Contains(transform)) return null;
            List<int> indexQueue = new List<int>();
            Transform current = child;
            do
            {
                indexQueue.Add(current.GetSiblingIndex());
                current = current.parent;
            }
            while (current != transform);
            indexQueue.Reverse();
            return indexQueue;
        }
        public static Transform FindChildIndexQueue(this Transform transform, List<int> indexQueue)
        {
            if (indexQueue.Count == 0) return null;
            foreach (var item in indexQueue)
            {
                transform = transform.GetChild(item);
                if (transform == null) return null;
            }
            return transform;
        }
    }
}
