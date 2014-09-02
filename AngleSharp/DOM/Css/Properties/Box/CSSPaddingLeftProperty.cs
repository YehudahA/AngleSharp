﻿namespace AngleSharp.DOM.Css.Properties
{
    /// <summary>
    /// Information can be found on MDN:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/padding-left
    /// </summary>
    sealed class CSSPaddingLeftProperty : CSSPaddingPartProperty, ICssPaddingLeftProperty
    {
        #region ctor

        internal CSSPaddingLeftProperty()
            : base(PropertyNames.PaddingLeft)
        {
        }

        #endregion
    }
}
