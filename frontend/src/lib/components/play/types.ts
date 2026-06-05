import type { PlayNodeDto } from '$lib/api/adventures';

export type ProgressState = 'completed' | 'current' | 'locked';

export type FlatNode = {
	node: PlayNodeDto;
	dayNumber: number;
	nodeIndexInDay: number;
	flatIndex: number;
	state: ProgressState;
	isFirstOfDay: boolean;
};
