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
    public class WWorld
    {
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
        private List<WObject> m_objectList;

        private RenderSystem m_renderSystem;
        private DebugDrawing m_gizmos;

        private Input m_input;
        private string m_worldName;

        public WWorld(string worldName)
        {
            m_worldName = worldName;
            m_componentList = new List<WComponent>();
            m_objectList = new List<WObject>();
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
            foreach(WComponent component in m_componentList)
            {
                component.Tick(deltaTime);
            }

            // Poll for new debug primitives.
            if(Map != null)
            {
                foreach(var room in Map.Rooms)
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

                if(Map.Stage != null)
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

        public void RegisterObject(WObject obj)
        {
            obj.World = this;
            m_objectList.Add(obj);
        }

        public void UnregisterObject(WObject obj)
        {
            m_objectList.Remove(obj);
        }

        public void UnloadWorld()
        {
            m_componentList.Clear();
            m_objectList.Clear();
            m_gizmos.ResetList();
            RenderSystem.UnloadAll();
        }

        public void DeleteSelectedObjects()
        {
            System.Console.WriteLine("Deleting {0} Selected Objects.", SelectedEntities.Count);
            
            if(Map == null)
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
            if(Map == null)
                throw new InvalidOperationException("Cannot raycast against an unloaded map!");

            List<RaycastHitResult> returnResults = new List<RaycastHitResult>();
            foreach(Room room in Map.Rooms)
            {
                foreach(var entity in room.Entities)
                {
                    if (!Map.LayerIsVisible(entity.Layer))
                        continue;

                    SceneComponent sceneEntity = entity as SceneComponent;
                    if (sceneEntity == null)
                        continue;


                    Vector3 aabbMin, aabbMax;
                    sceneEntity.GetAABB(out aabbMin, out aabbMax);

                    float intersectionDist;
                    if(RayIntersectsAABB(ray, aabbMin, aabbMax, out intersectionDist))
                    {
                        RaycastHitResult result = new RaycastHitResult();
                        result.Distance = intersectionDist;
                        result.Entity = entity;
                    }
                }
            }

            // Sort the results - nearest to furthest.
            returnResults = returnResults.OrderBy(x => x.Distance).ToList();
            return returnResults;
        }


        private static bool RayIntersectsAABB(Ray ray, Vector3 aabbMin, Vector3 aabbMax, out float intersectionDistance)
        {
            Vector3 dirFrac = new Vector3(1f / ray.Direction.X, 1f / ray.Direction.Y, 1f / ray.Direction.Z);

            float t1 = (aabbMin.X - ray.Origin.X) * dirFrac.X;
            float t2 = (aabbMax.X - ray.Origin.X) * dirFrac.X;
            float t3 = (aabbMin.Y - ray.Origin.Y) * dirFrac.Y;
            float t4 = (aabbMax.Y - ray.Origin.Y) * dirFrac.Y;
            float t5 = (aabbMin.Z - ray.Origin.Z) * dirFrac.Z;
            float t6 = (aabbMin.Z - ray.Origin.Z) * dirFrac.Z;

            float tMin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tMax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // If tMax < 0f, then ray intersects AABB but AABB is behind origin.
            if(tMax < 0f)
            {
                intersectionDistance = tMax;
                return false;
            }

            // If tMin > tMax, then ray doesn't intersect AABB.
            if(tMin > tMax)
            {
                intersectionDistance = tMax;
                return false;
            }

            // Finally we have an intersection.
            intersectionDistance = tMin;
            return true;
        }
    }
}
