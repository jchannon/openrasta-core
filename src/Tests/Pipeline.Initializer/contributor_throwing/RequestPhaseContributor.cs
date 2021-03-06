using System.Threading.Tasks;
using OpenRasta.Pipeline;

namespace Tests.Pipeline.Initializer.contributor_throwing
{
  class RequestPhaseContributor : KnownStages.IUriMatching
  {
    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.NotifyAsync(env => Task.FromResult(PipelineContinuation.Continue)).After<KnownStages.IBegin>();
    }
  }
}