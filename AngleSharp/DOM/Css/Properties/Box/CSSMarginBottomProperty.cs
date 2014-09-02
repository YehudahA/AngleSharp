﻿namespace AngleSharp.DOM.Css.Properties
{
    /// <summary>
    /// Information can be found on MDN:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/margin-bottom
    /// </summary>
    sealed class CSSMarginBottomProperty : CSSMarginPartProperty, ICssMarginBottomProperty
    {
        #region ctor

        internal CSSMarginBottomProperty()
            : base(PropertyNames.MarginBottom)
        {
        }

        #endregion
    }
}
