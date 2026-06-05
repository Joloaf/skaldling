import type { PlayDayDto, PlayNodeDto } from '$lib/api/adventures';

type ChunkBase = { id: string };
export type ChapterHeading = ChunkBase & { type: 'chapter-heading'; dayNumber: number };
export type IntroParagraph = ChunkBase & { type: 'intro'; text: string };
export type NarrativeParagraph = ChunkBase & { type: 'narrative'; text: string; nodeId: string };
export type TaskElement = ChunkBase & { type: 'task'; node: PlayNodeDto };
export type ConvergenceParagraph = ChunkBase & { type: 'convergence'; text: string };
export type Chunk =
	| ChapterHeading
	| IntroParagraph
	| NarrativeParagraph
	| TaskElement
	| ConvergenceParagraph;

export type Group = { id: string; chunks: Chunk[]; isChapterHeading: boolean };

function isNodeRevealed(node: PlayNodeDto, currentNodeId: string | null): boolean {
	return node.adventureTask.isCompleted || node.id === currentNodeId;
}
function isDayStarted(day: PlayDayDto, currentNodeId: string | null): boolean {
	return day.nodes.some((n) => isNodeRevealed(n, currentNodeId));
}
function isDayCompleted(day: PlayDayDto): boolean {
	return day.nodes.every((n) => n.adventureTask.isCompleted);
}

export function buildChunks(days: PlayDayDto[], currentNodeId: string | null): Chunk[] {
	const result: Chunk[] = [];
	for (const day of days) {
		if (!isDayStarted(day, currentNodeId)) continue;
		result.push({ id: `head-${day.id}`, type: 'chapter-heading', dayNumber: day.dayNumber });
		if (day.narrativeIntro) {
			result.push({ id: `intro-${day.id}`, type: 'intro', text: day.narrativeIntro });
		}
		for (const node of day.nodes) {
			if (!isNodeRevealed(node, currentNodeId)) continue;
			if (node.narrativeText) {
				result.push({
					id: `narr-${node.id}`,
					type: 'narrative',
					text: node.narrativeText,
					nodeId: node.id
				});
			}
			if (node.id === currentNodeId) {
				result.push({ id: `task-${node.id}`, type: 'task', node });
			}
		}
		if (isDayCompleted(day) && day.narrativeConvergence) {
			result.push({ id: `conv-${day.id}`, type: 'convergence', text: day.narrativeConvergence });
		}
	}
	return result;
}

export function buildGroups(chunks: Chunk[]): Group[] {
	const result: Group[] = [];
	for (let i = 0; i < chunks.length; i++) {
		const chunk = chunks[i];
		const next = chunks[i + 1];
		if (chunk.type === 'narrative' && next?.type === 'task' && next.node.id === chunk.nodeId) {
			result.push({ id: chunk.id, chunks: [chunk, next], isChapterHeading: false });
			i++;
		} else {
			result.push({
				id: chunk.id,
				chunks: [chunk],
				isChapterHeading: chunk.type === 'chapter-heading'
			});
		}
	}
	return result;
}

export function buildPages(
	chunks: Chunk[],
	assignments: Map<string, number>,
	totalPages: number
): Chunk[][] {
	const result: Chunk[][] = Array.from({ length: Math.max(totalPages, 1) }, () => [] as Chunk[]);
	for (const chunk of chunks) {
		const pageIdx = assignments.get(chunk.id);
		if (pageIdx !== undefined && pageIdx < result.length) {
			result[pageIdx].push(chunk);
		}
	}
	return result;
}
