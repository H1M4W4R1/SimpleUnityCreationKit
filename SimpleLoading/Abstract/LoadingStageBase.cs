using Systems.SimpleLoading.Data;
using System;

namespace Systems.SimpleLoading.Abstract
{
    /// <summary>Serializable configuration for one independently progressing loading task.</summary>
    /// <remarks>
    ///     Stages are regular C# objects stored by a sequence through <c>SerializeReference</c>. They are deliberately
    ///     not ScriptableObjects: a stage is owned by its sequence and produces isolated runtime work per request.
    /// </remarks>
    [Serializable]
    public abstract class LoadingStageBase
    {
        /// <summary>Relative contribution of this stage to total request progress.</summary>
        public virtual float TimeWeight => 1f;

        /// <summary>Creates isolated runtime work so the same stage asset can serve concurrent requests.</summary>
        public abstract ILoadingStageOperation CreateOperation(in LoadingContext context);
    }
}
