import initialise from './js/init-highlight';
import initialiseCopyCode from './js/copy-code';

const codeblock = document.querySelector('.js-codeblock');
if (codeblock) {
  initialise();
  initialiseCopyCode();
}
