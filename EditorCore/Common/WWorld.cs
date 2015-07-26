using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using WEditor.Maps;
using WEditor.Rendering;
using WEditor.WindWaker;
using System.Linq;

namespace WEditor
{
    public class WWorld : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> Handles rendering for objects associated with this WWorld. </summary>
        public RenderSystem RenderSystem
        {
            get { return m_renderSystem; }
        }

        public DebugDrawing Gizmos
        {
            get { return m_gizmos; }
        }

        public Input Input
        {
            get { return m_input; }
        }

        public string Name
        {
            get { return m_worldName; }
        }

        public Map Map { get; set; }
        public BindingList<MapEntity> SelectedEntities { get; set; }

        /// <summary> Used to calculate the delta time of the Tick loop. </summary>
        private Stopwatch m_dtStopwatch;

        /// <summary> List of components in the WWorld which need to recieve update ticks. </summary>
        private List<WComponent> m_componentList;

        private RenderSystem m_renderSystem;
        private DebugDrawing m_gizmos;

        private Input m_input;
        private string m_worldName;

        public WWorld(string worldName)
        {
            m_worldName = worldName;
            m_componentList = new List<WComponent>();
            m_dtStopwatch = new Stopwatch();
            m_input = new Input();

            m_gizmos = new DebugDrawing();
            m_renderSystem = new RenderSystem(this);
            SelectedEntities = new BindingList<MapEntity>();
        }

        public void InitializeSystem()
        {
            m_renderSystem.InitializeSystem();
            m_gizmos.InitializeSystem();
        }

        public void ShutdownSystem()
        {
            m_renderSystem.ShutdownSystem();
            m_gizmos.ShutdownSystem();
        }

        public void Tick()
        {
            float deltaTime = m_dtStopwatch.ElapsedMilliseconds / 1000f;
            m_dtStopwatch.Restart();
            m_gizmos.ResetList();

            // Update all of the entities in this world so they can move around, etc.
            foreach (WComponent component in m_componentList)
            {
                component.Tick(deltaTime);
            }

            // Poll for new debug primitives.
            if (Map != null)
            {
                foreach (var room in Map.Rooms)
                {
                    if (!room.Visible)
                        continue;

                    foreach (var ent in room.Entities)
                    {
                        if (!Map.LayerIsVisible(ent.Layer))
                            continue;

                        if (SelectedEntities.Contains(ent))
                            ent.OnDrawGizmosSelected();
                        else
                            ent.OnDrawGizmos();
                    }
                }

                if (Map.Stage != null)
                {
                    if (Map.Stage.Visible)
                    {
                        foreach (var ent in Map.Stage.Entities)
                        {
                            if (!Map.LayerIsVisible(ent.Layer))
                                continue;

                            if (SelectedEntities.Contains(ent))
                                ent.OnDrawGizmosSelected();
                            else
                                ent.OnDrawGizmos();
                        }
                    }
                }
            }

            // Badly placed hack to test stuff.
            if (Input.GetMouseButtonDown(0))
            {
                Ray mouseRay = m_renderSystem.m_editorCamera.ViewportPointToRay(Input.MousePosition);
                var hitResults = RaycastAll(mouseRay);
                Console.WriteLine("Hit {0} Objects.", hitResults.Count);
                for (int i = 0; i < hitResults.Count; i++)
                    Console.WriteLine("\t{0}", hitResults[i]);

                // If they're holding control, toggle the status of whether or not it is selected.
                if (Input.GetKey(System.Windows.Input.Key.LeftCtrl))
                {
                    foreach (var result in hitResults)
                    {

                        if (SelectedEntities.Contains(result))
                            SelectedEntities.Remove(result);
                        else
                            SelectedEntities.Add(result);
                    }
                }
                else
                {
                    SelectedEntities.Clear();
                    if (hitResults.Count > 0)
                        SelectedEntities.Add(hitResults[0]);
                }
            }

            // Finalize the debug primitive list.
            m_gizmos.FinalizePrimitiveBatch();

            // Update the render system this frame and draw everything.
            m_renderSystem.RenderFrame();

            // Update the internal input state so that we can compare against the previous state, ie: for first press, etc.
            m_input.Internal_UpdateInputState();
        }

        public void RegisterComponent(WComponent component)
        {
            component.World = this;
            component.Input = m_input;
            m_componentList.Add(component);
        }

        public void UnregisterComponent(WComponent component)
        {
            m_componentList.Remove(component);
        }

        public void UnloadWorld()
        {
            m_componentList.Clear();
            m_gizmos.ResetList();
            RenderSystem.UnloadAll();
        }

        public void DeleteSelectedObjects()
        {
            Console.WriteLine("Deleting {0} Selected Objects.", SelectedEntities.Count);

            if (Map == null)
                return;

            foreach (var room in Map.Rooms)
            {
                foreach (var entToRemove in SelectedEntities)
                    room.Entities.Remove(entToRemove);
            }

            if (Map.Stage == null)
                return;

            foreach (var entToRemove in SelectedEntities)
                Map.Stage.Entities.Remove(entToRemove);
        }

        /// <summary>
        /// Returns the closest result which intersects the Ray, or null if there is nothing.
        /// </summary>
        /// <param name="ray">Ray along which to check for intersections.</param>
        /// <returns>Closest MapEntity, or null if none.</returns>
        public MapEntity Raycast(Ray ray)
        {
            var allEnts = RaycastAllInternal(ray);
            if (allEnts.Count > 0)
                return allEnts[0].Entity;

            return null;
        }

        /// <summary>
        /// Returns all results (sorted in closest to furthest) order which intersect the <see cref="Ray"/>, or an 
        /// empty list if there are no results.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        public List<MapEntity> RaycastAll(Ray ray)
        {
            var allResults = RaycastAllInternal(ray);
            List<MapEntity> outputList = new List<MapEntity>();
            foreach (var result in allResults)
                outputList.Add(result.Entity);

            return outputList;
        }

        private struct RaycastHitResult
        {
            public MapEntity Entity;
            public float Distance;
        }

        private List<RaycastHitResult> RaycastAllInternal(Ray ray)
        {
            if (Map == null)
                throw new InvalidOperationException("Cannot raycast against an unloaded map!");

            List<RaycastHitResult> returnResults = new List<RaycastHitResult>();
            foreach (Room room in Map.Rooms)
            {
                foreach (var entity in room.Entities)
                {
                    if (!Map.LayerIsVisible(entity.Layer))
                        continue;

                    SceneComponent sceneEntity = entity as SceneComponent;
                    if (sceneEntity == null)
                        continue;


                    Vector3 aabbMin, aabbMax;
                    sceneEntity.GetAABB(out aabbMin, out aabbMax);

                    float intersectionDist;
                    if (RayIntersectsAABB(ray, aabbMin, aabbMax, out intersectionDist))
                    {
                        RaycastHitResult result = new RaycastHitResult();
                        result.Distance = intersectionDist;
                        result.Entity = entity;
                        returnResults.Add(result);
                    }
                }
            }

            // Sort the results - nearest to furthest.
            returnResults = returnResults.OrderBy(x => x.Distance).ToList();
            return returnResults;
        }


        private static bool RayIntersectsAABB(Ray ray, Vector3 aabbMin, Vector3 aabbMax, out float intersectionDistance)
        {
            Vector3 t_1 = new Vector3(), t_2 = new Vector3();

            float tNear = float.MinValue;
            float tFar = float.MaxValue;

            // Test infinite planes in each directin.
            for (int i = 0; i < 3; i++)
            {
                // Ray is parallel to planes in this direction.
                if (ray.Direction[i] == 0)
                {
                    if ((ray.Origin[i] < aabbMin[i]) || (ray.Origin[i] > aabbMax[i]))
                    {
                        // Parallel and outside of the box, thus no intersection is possible.
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
                else
                {
                    t_1[i] = (aabbMin[i] - ray.Origin[i]) / ray.Direction[i];
                    t_2[i] = (aabbMax[i] - ray.Origin[i]) / ray.Direction[i];

                    // Ensure T_1 holds values for intersection with near plane.
                    if (t_1[i] > t_2[i])
                    {
                        Vector3 temp = t_2;
                        t_2 = t_1;
                        t_1 = temp;
                    }

                    if (t_1[i] > tNear)
                        tNear = t_1[i];

                    if (t_2[i] < tFar)
                        tFar = t_2[i];

                    if ((tNear > tFar) || (tFar < 0))
                    {
                        intersectionDistance = float.MinValue;
                        return false;
                    }
                }
            }

            intersectionDistance = tNear;
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
