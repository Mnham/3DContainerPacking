import { copyFile, mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const demoAppRoot = path.join(repoRoot, 'src', 'CromulentBisgetti.DemoApp');

const asset = (from, to = from) => ({ from, to });
const stripSourceMapReference = content => content
  .replace(/\n?\/\*# sourceMappingURL=.*? \*\/\s*$/u, '')
  .replace(/\n?\/\/# sourceMappingURL=.*?\s*$/u, '');

const assets = [
  {
    ...asset(
      ['node_modules', 'bootstrap', 'dist', 'css', 'bootstrap.min.css'],
      ['wwwroot', 'lib', 'bootstrap', 'css', 'bootstrap.min.css']),
    transform: stripSourceMapReference
  },
  {
    ...asset(
      ['node_modules', 'bootstrap', 'dist', 'js', 'bootstrap.bundle.min.js'],
      ['wwwroot', 'lib', 'bootstrap', 'js', 'bootstrap.bundle.min.js']),
    transform: stripSourceMapReference
  },
  asset(
    ['node_modules', 'three', 'build', 'three.module.js'],
    ['wwwroot', 'lib', 'three', 'build', 'three.module.js']),
  asset(
    ['node_modules', 'three', 'build', 'three.core.js'],
    ['wwwroot', 'lib', 'three', 'build', 'three.core.js']),
  {
    from: ['node_modules', 'three', 'examples', 'jsm', 'controls', 'OrbitControls.js'],
    to: ['wwwroot', 'lib', 'three', 'examples', 'jsm', 'controls', 'OrbitControls.js'],
    transform: content => content.replace("} from 'three';", "} from '../../../build/three.module.js';")
  }
];

for (const asset of assets) {
  const source = path.join(repoRoot, ...asset.from);
  const destination = path.join(demoAppRoot, ...asset.to);

  await mkdir(path.dirname(destination), { recursive: true });

  if (asset.transform) {
    const content = await readFile(source, 'utf8');
    await writeFile(destination, asset.transform(content));
  } else {
    await copyFile(source, destination);
  }

  console.log(`${path.relative(repoRoot, source)} -> ${path.relative(repoRoot, destination)}`);
}
