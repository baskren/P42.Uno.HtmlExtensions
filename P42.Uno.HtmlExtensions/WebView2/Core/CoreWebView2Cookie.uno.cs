using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using P42.Net;
using Uno;
using Windows.Foundation.Metadata;

namespace P42.Web.WebView2.Core
{
    public enum CookieVariant
    {
        Unknown,
        Plain,
        Rfc2109,
        Rfc2965,
        Default = Rfc2109
    }

    public class CoreWebView2Cookie 
    {

        #region Fields
        internal const int MaxSupportedVersion = 1;
        internal const string MaxSupportedVersionString = "1";
        CookieVariant m_cookieVariant = CookieVariant.Plain; // Do not rename (binary serialization)
        string m_domain = string.Empty; // Do not rename (binary serialization)
        string m_path = string.Empty; // Do not rename (binary serialization)
        bool m_path_implicit = true; // Do not rename (binary serialization)
        string m_port = string.Empty; // Do not rename (binary serialization)
        bool m_port_implicit = true; // Do not rename (binary serialization)
        int m_version = MaxSupportedVersion; // Do not rename (binary serialization)

        string m_domainKey = string.Empty; // Do not rename (binary serialization)

        internal const string SeparatorLiteral = "; ";
        internal const char EqualsLiteral = '=';
        internal const string QuotesLiteral = "\"";
        internal const string SpecialAttributeLiteral = "$";

        internal static readonly char[] PortSplitDelimiters = new char[] { ' ', ',', '\"' };
        // Space (' ') should be reserved as well per RFCs, but major web browsers support it and some web sites use it - so we support it too
        internal static readonly char[] ReservedToName = new char[] { '\t', '\r', '\n', '=', ';', ',' };
        internal static readonly char[] ReservedToValue = new char[] { ';', ',' };

        #endregion

        public global::Microsoft.Web.WebView2.Core.CoreWebView2CookieSameSiteKind SameSite { get; set; }

        public bool IsSecure { get; set; }

        public bool IsHttpOnly { get; set; }

        public bool IsSession { get; internal set; }

        public string Comment { get; set; }

        public Uri CommentUri { get; set; }

        public bool HttpOnly { get; set; }

        public bool Discard { get; set; }

        internal bool DomainImplicit { get; set; } = true;


        public DateTime Expires { get; set; }

        public string Name { get; set; }


        internal bool Plain => Variant == CookieVariant.Plain;

        internal int[] PortList { get;  set; }

        public bool Secure { get; set; }

        public DateTime TimeStamp { get; internal set; } = DateTime.Now;

        public string Value { get; set; }

        public bool Expired
        {
            get => (Expires != DateTime.MinValue) && (Expires.ToLocalTime() <= DateTime.Now);
            set
            {
                if (value == true)
                    Expires = DateTime.Now;
            }
        }

        public string Path
        {
            get => m_path;
            set
            {
                m_path = value ?? string.Empty;
                m_path_implicit = false;
            }
        }

        public string Domain
        {
            get => m_domain;
            set
            {
                m_domain = value ?? string.Empty;
                DomainImplicit = false;
                m_domainKey = string.Empty; // _domainKey will be set when adding this cookie to a container.
            }
        }

        internal CookieVariant Variant => m_cookieVariant;

        // _domainKey member is set internally in VerifySetDefaults().
        // If it is not set then verification function was not called;
        // this should never happen.
        internal string DomainKey =>  DomainImplicit ? Domain : m_domainKey;

        public int Version
        {
            get => m_version;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                m_version = value;
                if (value > 0 && m_cookieVariant < CookieVariant.Rfc2109)
                    m_cookieVariant = CookieVariant.Rfc2109;

            }
        }

        public string Port
        {
            get => m_port;
            set
            {
                m_port_implicit = false;
                if (string.IsNullOrEmpty(value))
                {
                    // "Port" is present but has no value.
                    m_port = string.Empty;
                }
                else
                {
                    // Parse port list
                    if (value[0] != '\"' || value[value.Length - 1] != '\"')
                        throw new CookieException($"Invalid Cookie.Port value [{value}]");

                    string[] ports = value.Split(PortSplitDelimiters);

                    List<int> portList = new List<int>();
                    int port;

                    for (int i = 0; i < ports.Length; ++i)
                    {
                        // Skip spaces
                        if (ports[i] != string.Empty)
                        {
                            if (!int.TryParse(ports[i], out port))
                            {
                                throw new CookieException($"Invalid Cookie.Port value [{value}]");
                            }

                            // valid values for port 0 - 0xFFFF
                            if ((port < 0) || (port > 0xFFFF))
                            {
                                throw new CookieException($"Invalid Cookie.Port value [{value}]");
                            }

                            portList.Add(port);
                        }
                    }
                    PortList = portList.ToArray();
                    m_port = value;
                    m_version = MaxSupportedVersion;
                    m_cookieVariant = CookieVariant.Rfc2965;
                }
            }
        }

        internal CoreWebView2Cookie() {}

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new Exception("Cookie.Name cannot be null, empty or whitespace");

            string result = Name + EqualsLiteral + Value;
            if (!string.IsNullOrWhiteSpace(Comment))
                result += SeparatorLiteral + nameof(Comment) + EqualsLiteral + Comment;

            if (CommentUri != null)
                result += SeparatorLiteral + nameof(CommentUri) + EqualsLiteral + QuotesLiteral + CommentUri.ToString() + QuotesLiteral;

            if (Discard)
                result += SeparatorLiteral + nameof(Discard);

            if (!DomainImplicit && m_domain != null && m_domain.Length > 0)
            {
                result += SeparatorLiteral + nameof(Attribute) + EqualsLiteral + m_domain;
            }
            if (Expires != DateTime.MinValue)
            {
                int seconds = (int)(Expires.ToLocalTime() - DateTime.Now).TotalSeconds;
                if (seconds < 0)
                {
                    // This means that the cookie has already expired. Set Max-Age to 0
                    // so that the client will discard the cookie immediately.
                    seconds = 0;
                }
                result += SeparatorLiteral + nameof(Expires) + EqualsLiteral + seconds.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
            }
            if (!m_path_implicit && m_path != null && m_path.Length > 0)
            {
                result += SeparatorLiteral + nameof(Path) + EqualsLiteral + m_path;
            }
            if (!Plain && !m_port_implicit && m_port != null && m_port.Length > 0)
                // QuotesLiteral are included in _port.
                result += SeparatorLiteral + nameof(Port) + EqualsLiteral + m_port;

            if (Version > 0)
                result += SeparatorLiteral + nameof(Version) + EqualsLiteral + Version.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

            return result == "=" ? null : result;
        }

        internal CoreWebView2Cookie Clone()
        {
            return  new CoreWebView2Cookie
            {
                Name = Name,
                Value = Value,
                Path = Path,
                Domain = Domain,
                Port = Port,
                Expires = Expires,
                IsSecure = IsSecure,
                IsHttpOnly = IsHttpOnly,
                IsSession = IsSession,
                Comment = Comment,
                CommentUri = CommentUri,
                HttpOnly = HttpOnly,
                DomainImplicit = DomainImplicit,
                PortList = new List<int>(PortList).ToArray(),
                Secure = Secure,
                TimeStamp = TimeStamp,
                Discard = Discard,
                SameSite = SameSite,
                Version= Version,
            };


        }
    }
}
