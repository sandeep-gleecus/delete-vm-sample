using System.Collections.Generic;
using System.Linq;

using Validation.Reports.Windward.Core.Repository;

namespace Validation.Reports.Windward.Core.Model
{
	public class TemplateModel
	{
		private TemplateRepository _repo = new TemplateRepository();

		public TemplateModel()
		{
			LoadTemplates();
		}

		public List<Template> _templates;

		public List<Template> StandardTemplates
		{
			get { return _templates.Where(x => x.IsCustom == false).OrderBy(x => x.CategoryGroup).ThenByDescending(x => x.ReportCategory).ToList<Template>(); }
		}

		public List<Template> CustomTemplates
		{
			get { return _templates.Where(x => x.IsCustom == true).ToList<Template>(); }
		}

		public List<TemplateOutputType> GetTemplateOutputTypes(int templateId)
		{
			List<TemplateOutputType> results = new List<TemplateOutputType>();

			results = _templates.Where(x => x.TemplateId == templateId)
								.SelectMany(x => x.OutputTypes)
								.ToList();

			return results;
		}

		private void LoadTemplates()
		{
			List<Template> result = _repo.GetAllTemplates().ToList<Template>();
			_templates = result;
		}


		public List<TemplateParameter> GetTemplateParameters(int templateId)
		{
			return _templates.Where(x => x.TemplateId == templateId)
							 .SelectMany(x => x.Parameters)
							 .ToList();
		}

		internal void InsertTemplate(Template newTemplate)
		{
			_repo.Create(newTemplate);

		}

		public void DeleteTemplate(int templateid)
		{
			_repo.Delete(templateid);
		}
	}
}
