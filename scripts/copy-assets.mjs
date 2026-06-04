import { copyFile, mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const demoAppRoot = path.join(repoRoot, 'src', 'CromulentBisgetti.DemoApp');

const assets = [
  {
    from: ['node_modules', 'bootstrap', 'dist', 'css', 'bootstrap.min.css'],
    to: ['wwwroot', 'lib', 'bootstrap', 'css', 'bootstrap.min.css']
  },
  {
    from: ['node_modules', 'bootstrap', 'dist', 'css', 'bootstrap.min.css.map'],
    to: ['wwwroot', 'lib', 'bootstrap', 'css', 'bootstrap.min.css.map']
  },
  {
    from: ['node_modules', 'bootstrap', 'dist', 'js', 'bootstrap.bundle.min.js'],
    to: ['wwwroot', 'lib', 'bootstrap', 'js', 'bootstrap.bundle.min.js']
  },
  {
    from: ['node_modules', 'bootstrap', 'dist', 'js', 'bootstrap.bundle.min.js.map'],
    to: ['wwwroot', 'lib', 'bootstrap', 'js', 'bootstrap.bundle.min.js.map']
  },
  {
    from: ['node_modules', 'three', 'build', 'three.module.js'],
    to: ['wwwroot', 'lib', 'three', 'build', 'three.module.js']
  },
  {
    from: ['node_modules', 'three', 'build', 'three.core.js'],
    to: ['wwwroot', 'lib', 'three', 'build', 'three.core.js']
  },
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
