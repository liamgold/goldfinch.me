(window.webpackJsonp=window.webpackJsonp||[]).push([[8],{"9hGI":function(e,n,t){"use strict";t.r(n),t.d(n,"query",(function(){return h}));var l=t("MUpH"),a=t("q1tI"),r=t.n(a),i=t("TjXv"),u=t("soUV"),o=t("Wbzz"),m="2560px",c={mobileS:"(min-width: "+"320px"+")",mobileM:"(min-width: "+"375px"+")",mobileL:"(min-width: "+"425px"+")",tablet:"(min-width: "+"768px"+")",laptop:"(min-width: "+"1024px"+")",laptopL:"(min-width: "+"1440px"+")",desktop:"(min-width: "+m+")",desktopL:"(min-width: "+m+")"};function d(){var e=Object(l.a)(["\n  display: flex;\n  flex-direction: column;\n  background-color: ",";\n  padding: 25px;\n  border-radius: 8px;\n  margin-bottom: 25px;\n\n  @media "," {\n    flex: 0 1 49%;\n  }\n"]);return d=function(){return e},e}function s(){var e=Object(l.a)(["\n  font-size: 14px;\n"]);return s=function(){return e},e}var v=i.a.span(s()),p=Object(i.a)((function(e){var n,t,l,a,i,u,m,c,d=e.className,s=e.blog,p=new Date((null==s||null===(n=s.elements)||void 0===n||null===(t=n.post_date)||void 0===t?void 0:t.value)||""),f=new Intl.DateTimeFormat("default",{year:"numeric",month:"numeric",day:"numeric"}).format(p);return r.a.createElement("div",{className:d},r.a.createElement(o.Link,{to:"/blog/"+(null==s||null===(l=s.elements)||void 0===l||null===(a=l.url_slug)||void 0===a?void 0:a.value)+"/"},null==s||null===(i=s.elements)||void 0===i||null===(u=i.base__title)||void 0===u?void 0:u.value),r.a.createElement(v,null,""+f),r.a.createElement("p",null,null==s||null===(m=s.elements)||void 0===m||null===(c=m.summary)||void 0===c?void 0:c.value))}))(d(),(function(e){return e.theme.colour.grey}),c.tablet),f=t("qhky");function b(){var e=Object(l.a)(["\n  ul {\n    display: flex;\n    flex-wrap: wrap;\n    justify-content: center;\n    list-style: none;\n    padding: 0;\n    margin: 0;\n\n    li {\n      margin: 0 0 0 15px;\n    }\n  }\n"]);return b=function(){return e},e}var g=Object(i.a)((function(e){var n=e.className,t=e.baseUrl,l=e.currentPage,i=e.numPages,u=Object(o.useStaticQuery)(x).site.siteMetadata.siteUrl,m=1===l,c=l===i,d=l-1==1?""+u+t:""+u+t+(l-1)+"/",s=""+u+t+(l+1)+"/";return r.a.createElement(a.Fragment,null,r.a.createElement(f.Helmet,null,!m&&r.a.createElement("link",{rel:"prev",href:d}),!c&&r.a.createElement("link",{rel:"next",href:s})),r.a.createElement("div",{className:n},r.a.createElement("ul",null,!m&&r.a.createElement("li",null,r.a.createElement(o.Link,{to:d,rel:"prev"},"Previous")),Array.from({length:i},(function(e,n){return r.a.createElement("li",{key:"pagination-number"+(n+1)},r.a.createElement(o.Link,{to:t+(0===n?"":n+1+"/")},n+1))})),!c&&r.a.createElement("li",null,r.a.createElement(o.Link,{to:s,rel:"next"},"Next")))))}))(b()),x="764153399";function E(){var e=Object(l.a)(["\n  display: flex;\n  flex-wrap: wrap;\n  justify-content: space-between;\n"]);return E=function(){return e},e}var w=i.a.div(E()),h=(n.default=function(e){var n,t,l,a,i,o,m=e.data,c=e.pageContext,d=m.kontentItemBlogListing,s=m.allKontentItemBlogDetail;if(!d)return null;var v=c.currentPage,f=c.numPages;return r.a.createElement(u.a,{title:null==d||null===(n=d.elements)||void 0===n||null===(t=n.seo__meta_title)||void 0===t?void 0:t.value,description:null==d||null===(l=d.elements)||void 0===l||null===(a=l.seo__meta_description)||void 0===a?void 0:a.value},r.a.createElement("div",null,r.a.createElement("h1",null,null==d||null===(i=d.elements)||void 0===i||null===(o=i.base__title)||void 0===o?void 0:o.value),r.a.createElement(w,null,s.edges.map((function(e){var n=null==e?void 0:e.node;return r.a.createElement(p,{key:n.system.id,blog:n})}))),r.a.createElement(g,{baseUrl:"/blog/",currentPage:v,numPages:f})))},"2530925683")}}]);
//# sourceMappingURL=component---src-templates-blog-post-listing-tsx-c911352f9aefd08c7f99.js.map