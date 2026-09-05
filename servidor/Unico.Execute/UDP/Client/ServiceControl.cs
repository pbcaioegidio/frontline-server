using Executable.Supervision;
using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.Network;
using System.Threading;

namespace Executable.UDP.Client
{
    public class ServiceControl
    {
        public static void Load(SyncClientPacket C)
        {
            byte action = C.ReadC();
            byte serviceId = C.ReadC();
            string name = serviceId == 1 ? "auth" : serviceId == 2 ? "game" : serviceId == 3 ? "match" : null;
            if (name == null)
            {
                CLogger.Print($"[ServiceControl] Unknown serviceId: {serviceId}", LoggerType.Warning);
                return;
            }
            if (!ConfigLoader.ProcessSplit)
            {
                CLogger.Print($"[ServiceControl] Ignored action={action} service={name}: ProcessSplit is disabled.", LoggerType.Warning);
                return;
            }
            CLogger.Print($"[ServiceControl] action={action} service={name} accepted.", LoggerType.Command);
            new Thread(() =>
            {
                switch (action)
                {
                    case 1: ProcessSupervisor.Instance.Start(name); break;
                    case 2: ProcessSupervisor.Instance.Stop(name); break;
                    case 3: ProcessSupervisor.Instance.Restart(name); break;
                    default: CLogger.Print($"[ServiceControl] Unknown action: {action}", LoggerType.Warning); break;
                }
            })
            { IsBackground = true }.Start();
        }
    }
}
