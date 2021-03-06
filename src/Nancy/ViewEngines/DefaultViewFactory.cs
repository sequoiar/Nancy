﻿namespace Nancy.ViewEngines
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The default implementation for how views are resolved and rendered by Nancy.
    /// </summary>
    public class DefaultViewFactory : IViewFactory
    {
        private readonly IViewResolver viewResolver;
        private readonly IEnumerable<IViewEngine> viewEngines;
        private static readonly Action<Stream> EmptyView = x => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultViewFactory"/> class.
        /// </summary>
        /// <param name="viewResolver">An <see cref="IViewResolver"/> instance that should be used to resolve the location of a view.</param>
        /// <param name="viewEngines">An <see cref="IEnumerable{T}"/> instance containing the <see cref="IViewEngine"/> instances that should be able to be used to render a view</param>
        public DefaultViewFactory(IViewResolver viewResolver, IEnumerable<IViewEngine> viewEngines)
        {
            this.viewResolver = viewResolver;
            this.viewEngines = viewEngines;
        }

        /// <summary>
        /// Renders the view with the name and model defined by the <paramref name="viewName"/> and <paramref name="model"/> parameters.
        /// </summary>
        /// <param name="viewName">The name of the view to render.</param>
        /// <param name="model">The model that should be passed into the view.</param>
        /// <param name="viewLocationContext">A <see cref="ViewLocationContext"/> instance, containing information about the context for which the view is being rendered.</param>
        /// <returns>A delegate that can be invoked with the <see cref="Stream"/> that the view should be rendered to.</returns>
        public Action<Stream> RenderView(string viewName, dynamic model, ViewLocationContext viewLocationContext)
        {
            if (viewName == null && model == null)
            {
                throw new ArgumentException("View name and model parameters cannot both be null.");
            }

            if (model == null && viewName.Length == 0)
            {
                throw new ArgumentException("The view name parameter cannot be empty when the model parameters is null.");
            }

            if (viewLocationContext == null)
            {
                throw new ArgumentNullException("viewLocationContext", "The value of the viewLocationContext parameter cannot be null.");
            }

            var actualViewName = 
                viewName ?? GetViewNameFromModel(model);

            return this.GetRenderedView(actualViewName, model, viewLocationContext);
        }

        private Action<Stream> GetRenderedView(string viewName, dynamic model, ViewLocationContext viewLocationContext)
        {
            var viewLocationResult =
                this.viewResolver.GetViewLocation(viewName, model, viewLocationContext);

            var resolvedViewEngine = 
                GetViewEngine(viewLocationResult);

            if (resolvedViewEngine == null)
            {
                return EmptyView;
            }

            return SafeInvokeViewEngine(
                resolvedViewEngine,
                viewLocationResult,
                model
            );
        }

        private IViewEngine GetViewEngine(ViewLocationResult viewLocationResult)
        {
            if (viewLocationResult == null)
            {
                return null;
            }

            var matchingViewEngines = 
                from viewEngine in this.viewEngines
                where viewEngine.Extensions.Any(x => x.Equals(viewLocationResult.Extension, StringComparison.InvariantCultureIgnoreCase))
                select viewEngine;

            return matchingViewEngines.FirstOrDefault();
        }

        private static string GetViewNameFromModel(dynamic model)
        {
            return Regex.Replace(model.GetType().Name, "Model$", string.Empty);
        }

        private static Action<Stream> SafeInvokeViewEngine(IViewEngine viewEngine, ViewLocationResult locationResult, dynamic model)
        {
            try
            {
                return viewEngine.RenderView(locationResult, model);
            }
            catch (Exception)
            {
                return EmptyView;
            }
        }
    }
}