using System.Collections.Generic;


namespace Gley.UrbanSystem.Internal
{
    public class DestroyableManager
    {
        private static DestroyableManager _instance;
        public static DestroyableManager Instance => _instance ??= new DestroyableManager();

        private List<IDestroyable> _destroyables = new List<IDestroyable>();


        public void Register(IDestroyable destroyable)
        {
            _destroyables.Add(destroyable);
        }


        public void Unregister(IDestroyable destroyable)
        {
            _destroyables.Remove(destroyable);
        }


        public void DestroyAll()
        {
            foreach (var destroyable in _destroyables)
            {
                destroyable.OnDestroy();
            }
            _destroyables = new List<IDestroyable>();
        }
    }

}
