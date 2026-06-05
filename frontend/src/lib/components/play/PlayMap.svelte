<script lang="ts">
	import type { FlatNode, ProgressState } from './types';

	type Props = {
		flatNodes: FlatNode[];
		onNodeClick?: (nodeId: string) => void;
	};

	const { flatNodes, onNodeClick }: Props = $props();

	const MAP_WIDTH = 500;
	const MAP_HEIGHT = 800;
	const PADDING = 80;
	const NODE_RADIUS = 22;

	type Positioned = FlatNode & { position: { x: number; y: number } };

	// Placing all nodes on the map.
	const positioned: Positioned[] = $derived.by(() => {
		const total = flatNodes.length;
		return flatNodes.map((fn, i) => {
			const t = total > 1 ? i / (total - 1) : 0;
			// First node at the bottom, last/finale node at the top.
			const y = MAP_HEIGHT - PADDING - t * (MAP_HEIGHT - 2 * PADDING);
			// Spread each node across the x-axis to create a non-straight path.
			const x =
				MAP_WIDTH / 2 + Math.sin(i * 1.2) * (MAP_WIDTH / 2 - PADDING - NODE_RADIUS);
			return { ...fn, position: { x, y } };
		});
	});

	// Seeded placement of trees across the map, ensuring the same location for trees on every mount.
	const treePositions = (() => {
		const trees: { x: number; y: number; size: number }[] = [];
		let seed = 1234567;
		const rand = () => {
			seed = (seed * 1103515245 + 12345) & 0x7fffffff;
			return (seed % 1000) / 1000;
		};
		for (let i = 0; i < 60; i++) {
			let x: number, y: number;
			if (i < 45) {
				const side = Math.floor(rand() * 4);
				if (side === 0) { x = rand() * MAP_WIDTH; y = rand() * 70; }
				else if (side === 1) { x = MAP_WIDTH - 70 + rand() * 70; y = rand() * MAP_HEIGHT; }
				else if (side === 2) { x = rand() * MAP_WIDTH; y = MAP_HEIGHT - 70 + rand() * 70; }
				else { x = rand() * 70; y = rand() * MAP_HEIGHT; }
			} else {
				x = rand() * MAP_WIDTH;
				y = rand() * MAP_HEIGHT;
				if (Math.abs(x - MAP_WIDTH / 2) < 120) x = x < MAP_WIDTH / 2 ? x - 80 : x + 80;
			}
			trees.push({ x, y, size: 16 + rand() * 14 });
		}
		return trees;
	})();

	function getSceneIcon(sceneType: string | null): string {
		switch (sceneType) {
			case 'Quest': return '⚔';
			case 'Encounter': return '👥';
			case 'Discovery': return '✦';
			case 'Reflection': return '◐';
			case 'Climax': return '⚡';
			case 'Resolution': return '★';
			default: return '?';
		}
	}

	function getNodeFill(state: ProgressState): string {
		switch (state) {
			case 'completed': return '#4a7c4a';
			case 'current': return '#c89942';
			case 'locked': return '#555';
		}
	}

	function handleNodeClick(fn: Positioned) {
		if (fn.state === 'locked') return;
		onNodeClick?.(fn.node.id);
	}
</script>

<div class="map-container">
	<svg viewBox="0 0 {MAP_WIDTH} {MAP_HEIGHT}" class="map" preserveAspectRatio="xMidYMid meet">
		<defs>
			<radialGradient id="parchment" cx="50%" cy="60%" r="70%">
				<stop offset="0%" stop-color="#f5e8c4" />
				<stop offset="100%" stop-color="#b39752" />
			</radialGradient>
		</defs>

		<rect width={MAP_WIDTH} height={MAP_HEIGHT} fill="url(#parchment)" />

		{#each treePositions as tree, i (i)}
			<text x={tree.x} y={tree.y} font-size={tree.size} opacity="0.9">🌲</text>
		{/each}

		{#each positioned as fn, i (fn.node.id + '-path')}
			{#if i > 0}
				{@const prev = positioned[i - 1]}
				<line
					x1={prev.position.x}
					y1={prev.position.y}
					x2={fn.position.x}
					y2={fn.position.y}
					stroke="#2d2d2d"
					stroke-width="3"
					stroke-dasharray="8 6"
					opacity={fn.state === 'locked' ? 0.3 : 0.75} />
			{/if}
		{/each}

		{#each positioned as fn (fn.node.id)}
			<g
				class="node-marker"
				class:clickable={fn.state !== 'locked' && onNodeClick}
				class:pulsing={fn.state === 'current'}
				onclick={() => handleNodeClick(fn)}
				role={fn.state !== 'locked' && onNodeClick ? 'button' : undefined}
				tabindex={fn.state !== 'locked' && onNodeClick ? 0 : undefined}
				onkeydown={(e) => {
					if (e.key === 'Enter' || e.key === ' ') {
						e.preventDefault();
						handleNodeClick(fn);
					}
				}}>
				<circle
					cx={fn.position.x}
					cy={fn.position.y}
					r={NODE_RADIUS}
					fill={getNodeFill(fn.state)}
					stroke={fn.state === 'current' ? '#fff' : '#2d2d2d'}
					stroke-width={fn.state === 'current' ? 3 : 2} />
				<text
					x={fn.position.x}
					y={fn.position.y + 7}
					text-anchor="middle"
					fill="white"
					font-size="20"
					font-weight="600"
					pointer-events="none">
					{#if fn.state === 'completed'}✓
					{:else if fn.state === 'locked'}🔒
					{:else}{getSceneIcon(fn.node.sceneType)}{/if}
				</text>
			</g>
		{/each}
	</svg>
</div>

<style>
    .map-container {
        border: 2px solid #2d2d2d;
        border-radius: 4px;
        overflow: hidden;
        background: #2d4a2d;
    }
    .map {
        display: block;
        width: 100%;
        height: auto;
    }
    .node-marker.clickable {
        cursor: pointer;
    }
    .node-marker.pulsing circle {
        animation: pulse 2s ease-in-out infinite;
    }
    @keyframes pulse {
        0%, 100% {
            filter: drop-shadow(0 0 0 rgba(255, 255, 255, 0.4));
        }
        50% {
            filter: drop-shadow(0 0 8px rgba(255, 255, 255, 0.8));
        }
    }
</style>