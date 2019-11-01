using RPCExp.Common;
using RPCExp.TagLogger.Entities;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.TagLogger
{
    public class TagLogManager : ServiceAbstract
    {
        TagLogContext context;

        public TagLogManager()
        {

        }

        ~TagLogManager() =>
            context?.SaveChanges();

        public int ItemsToSaveLimit { get; private set; } = 1;

        private TagLogContext Context {
            get {
                if (context == default)
                    context = new TagLogContext();
                return context;
            }
        }

        public List<TagLogConfig> Configs { get; set; } = new List<TagLogConfig>();

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            int itemsToSaveCount = 0;
            foreach (var cfg in Configs)
            {
                var storedTagLogInfo = Context.TagLogInfo.FirstOrDefault(e =>
                    e.FacilityAccessName == cfg.TagLogInfo.FacilityAccessName &&
                    e.DeviceName == cfg.TagLogInfo.DeviceName &&
                    e.TagName == cfg.TagLogInfo.TagName);

                if (storedTagLogInfo == default)
                {
                    storedTagLogInfo = new TagLogInfo
                    {
                        FacilityAccessName = cfg.TagLogInfo.FacilityAccessName,
                        DeviceName = cfg.TagLogInfo.DeviceName,
                        TagName = cfg.TagLogInfo.TagName,
                    };
                    Context.TagLogInfo.Add(storedTagLogInfo);
                    itemsToSaveCount++;
                }
                cfg.TagLogInfo = storedTagLogInfo;
            }

            if (itemsToSaveCount >= ItemsToSaveLimit)
            {
                await Context.SaveChangesAsync(cancellationToken);
                itemsToSaveCount = 0;
            }

            // Главный цикл
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach(var cfg in Configs)
                {
                    var ad = cfg.NeedToArcive;
                    if (ad != default)
                        await Context.TagLogData.AddAsync(new TagLogData { });
                }

                if(itemsToSaveCount >= ItemsToSaveLimit)
                {
                    await Context.SaveChangesAsync(cancellationToken);
                    itemsToSaveCount = 0;
                }
            }
        }
    }
}
