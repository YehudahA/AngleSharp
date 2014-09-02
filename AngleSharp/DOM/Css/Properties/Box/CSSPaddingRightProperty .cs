﻿namespace AngleSharp.DOM.Css.Properties
{
    /// <summary>
    /// Information can be found on MDN:
    /// https://developer.mozilla.org/en-US/docs/Web/CSS/padding-right
    /// </summary>
    sealed class CSSPaddingRightProperty : CSSPaddingPartProperty, ICssPaddingRightProperty
    {
        #region ctor

        internal CSSPaddingRightProperty()
            : base(PropertyNames.PaddingRight)
        {
        }

        #endregion
    }
}
