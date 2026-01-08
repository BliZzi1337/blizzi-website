import fs from 'fs';
import { execSync } from 'child_process';
import path from 'path';

// Stelle sicher, dass das src/data Verzeichnis existiert
const dataDir = 'src/data';
if (!fs.existsSync(dataDir)) {
  fs.mkdirSync(dataDir, { recursive: true });
}

// Hole die Commits mit spezifischen Nachrichten
const getMostRecentCommitDate = (message) => {
  try {
    const cmd = `git log --all --grep="${message}" --format="%aI" --max-count=1`;
    const date = execSync(cmd, { encoding: 'utf-8' }).trim();
    return date || null;
  } catch (error) {
    console.warn(`⚠️  Warnung beim Abrufen von Commits für "${message}": ${error.message}`);
    return null;
  }
};

const timestamps = {
  marbles: getMostRecentCommitDate('Update marbles leaderboard'),
  gold: getMostRecentCommitDate('Update gold leaderboard'),
  lastGenerated: new Date().toISOString()
};

// Speichere in JSON Datei
fs.writeFileSync(
  path.join(dataDir, 'timestamps.json'),
  JSON.stringify(timestamps, null, 2)
);

console.log('✅ Timestamps aktualisiert:');
console.log(`   🔮 Marbles: ${timestamps.marbles || 'Nie'}`);
console.log(`   💰 Gold: ${timestamps.gold || 'Nie'}`);