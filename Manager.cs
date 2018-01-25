using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tastenhacker.Core.Manager
{
    public abstract class ManagerModule : MonoBehaviour
    {
        private void Awake()
        {
            Manager.Instance.Register(this);
            OnAwake();
        }

        public virtual void OnNewScene()
        {
        }

        private void OnDestroy()
        {
            if (Manager.Instance != null)
            {
                Manager.Instance.Unregister(this);
            }
        }

        public virtual void OnAwake() { }
    }

    public abstract class StaticManagerModule : ManagerModule { }

    public abstract class StaticManagerModule<T> : StaticManagerModule where T : StaticManagerModule<T>
    {
    }

    public class ModuleAlreadyRegisteredException : Exception
    {
        public ModuleAlreadyRegisteredException(string message)
            : base(message)
        {
        }
    }

    public class ModuleNotFoundException : Exception { }

    public class Manager : MonoBehaviour
    {
        public static Manager Instance;

        [SerializeField]
        private List<ManagerModule> _modules;

        private void Awake()
        {
            if (Instance != null)
            {
                throw new ArgumentException();
            }
            Instance = this;
        }

        public void Register(ManagerModule managerModule)
        {
            if (ModuleRegistered(managerModule.GetType()))
            {
                throw new ModuleAlreadyRegisteredException(string.Format("{0} is already registered", managerModule.GetType().Name));
            }
            _modules.Add(managerModule);
        }

        public void Unregister(ManagerModule managerModule)
        {
            if (!ModuleRegistered(managerModule.GetType()))
            {
                throw new ModuleNotFoundException();
            }
            _modules.Remove(managerModule);
        }

        private bool ModuleRegistered(Type t)
        {
            return _modules.Any(m => m.GetType() == t);
        }

        public bool ModuleRegistered<T>()
        {
            return ModuleRegistered(typeof(T));
        }

        public T Get<T>() where T : ManagerModule
        {
            if (!ModuleRegistered(typeof(T)))
            {
                if (typeof(T).IsSubclassOf(typeof(StaticManagerModule)))
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    go.transform.parent = Instance.gameObject.transform;
                    return (T)go.AddComponent(typeof(T));
                }
                throw new ModuleNotFoundException();
            }
            return _modules.Single(m => m.GetType() == typeof(T)) as T;
        }

        private void OnLevelWasLoaded(int level)
        {
            foreach (ManagerModule module in _modules)
            {
                module.OnNewScene();
            }
        }
    }
}