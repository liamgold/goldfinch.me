import hljs from 'highlight.js/lib/core';
import csharp from 'highlight.js/lib/languages/csharp';
import javascript from 'highlight.js/lib/languages/javascript';
import typescript from 'highlight.js/lib/languages/typescript';
import yaml from 'highlight.js/lib/languages/yaml';
import css from 'highlight.js/lib/languages/css';
import xml from 'highlight.js/lib/languages/xml';
import bash from 'highlight.js/lib/languages/bash';
import 'highlight.js/scss/vs2015.scss';

hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('javascript', javascript);
hljs.registerLanguage('typescript', typescript);
hljs.registerLanguage('css', css);
hljs.registerLanguage('yaml', yaml);
hljs.registerLanguage('xml', xml);
hljs.registerLanguage('bash', bash);

const initialise = () => {
  hljs.highlightAll();
};

export default initialise;
